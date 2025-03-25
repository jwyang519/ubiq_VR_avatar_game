using System;
using UnityEngine;
using Ubiq.Messaging;
using UnityEngine.InputSystem;
using Ubiq.Avatars;
using Ubiq.Rooms; // Needed for accessing RoomClient

public class AvatarPartNetworkSync : MonoBehaviour
{
    private NetworkContext context;
    private AvatarPartSetter partSetter;
    private RoomClient roomClient; // Reference to the RoomClient
    private string avatarPartKey;  // Unique key to store avatar customization

    private float lastPingTime = 0f;
    private float pingCooldown = 2f; // seconds

    private void Start()
    {
        var avatar = GetComponent<Ubiq.Avatars.Avatar>();
        if (avatar == null)
        {
            Debug.LogError("[AvatarPartNetworkSync] Avatar component not found on this GameObject.");
            return;
        }

        context = NetworkScene.Register(this, NetworkId.Create(avatar.NetworkId, nameof(AvatarPartNetworkSync)));
        Debug.Log($"[AvatarPartNetworkSync] Registered with id: {context.GetHashCode()}");

        roomClient = GetComponentInParent<RoomClient>();
        if (roomClient == null)
        {
            Debug.LogError("[AvatarPartNetworkSync] RoomClient not found in parent hierarchy.");
            return;
        }

        avatarPartKey = "avatarPart_" + avatar.NetworkId.ToString();
        roomClient.OnRoomUpdated.AddListener(OnRoomUpdated);

        Debug.LogError($"[AvatarPartNetworkSync] AvatarPartKey: {avatarPartKey}");

        partSetter = GetComponent<AvatarPartSetter>();
        if (partSetter == null)
        {
            Debug.LogError("[AvatarPartNetworkSync] AvatarPartSetter component not found on this avatar.");
        }
        else
        {
            Debug.Log("[AvatarPartNetworkSync] Found AvatarPartSetter component.");
        }

        // Immediately apply any persisted customization from the room.
        ApplyPersistedCustomization();
    }

    private void Update()
    {
        // Example: Using the new Input System to check for the "P" key.
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (Time.time - lastPingTime > pingCooldown)
            {
                SendPing();
                lastPingTime = Time.time;
            }
            else
            {
                Debug.Log("Ping cooldown active. Not sending ping.");
            }
        }
    }

    public void SendPing()
    {
        var ping = new TestMessage() { text = "ping" };
        string json = JsonUtility.ToJson(ping);
        Debug.Log("[AvatarPartNetworkSync] Sending ping message: " + json);
        context.SendJson(ping);
    }

    // This method will be called when the room state is updated.
    private void OnRoomUpdated(IRoom room)
    {
        var partData = room[avatarPartKey];
        if (!string.IsNullOrEmpty(partData))
        {
            var msg = JsonUtility.FromJson<PartMessage>(partData);
            if (msg != null && !string.IsNullOrEmpty(msg.category))
            {
                Debug.Log($"[AvatarPartNetworkSync] Applying persisted change: {msg.category} -> {msg.partName}");
                partSetter?.SetPart(msg.category, msg.partName);
            }
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        Debug.Log($"[AvatarPartNetworkSync] ProcessMessage called on {gameObject.name}");
        string rawMessage = message.ToString();
        Debug.Log($"[AvatarPartNetworkSync] Received raw message: {rawMessage}");

        var partMsg = message.FromJson<PartMessage>();
        if (partMsg != null && !string.IsNullOrEmpty(partMsg.category))
        {
            Debug.Log($"[AvatarPartNetworkSync] Parsed PartMessage: category = {partMsg.category}, partName = {partMsg.partName}");
            ApplyRemoteChange(partMsg);
            return;
        }

        var testMsg = message.FromJson<TestMessage>();
        if (testMsg != null && !string.IsNullOrEmpty(testMsg.text))
        {
            Debug.Log($"[AvatarPartNetworkSync] Received TestMessage: {testMsg.text}");
        }
        else
        {
            Debug.LogError("[AvatarPartNetworkSync] Failed to parse incoming message.");
        }
    }

    private void ApplyRemoteChange(PartMessage msg)
    {
        if (partSetter == null)
        {
            Debug.LogError("[AvatarPartNetworkSync] Cannot apply remote change because AvatarPartSetter is null.");
            return;
        }
        Debug.Log($"[AvatarPartNetworkSync] Applying remote change: {msg.category} -> {msg.partName}");
        partSetter.SetPart(msg.category, msg.partName);
    }

    // This method is called by your UI when a user selects a new part.
    public void SetPartNetworked(string category, string partName)
    {
        Debug.Log($"[AvatarPartNetworkSync] Local SetPartNetworked called with category: {category}, partName: {partName}");

        // 1) Update locally using the AvatarPartSetter.
        if (partSetter != null)
        {
            partSetter.SetPart(category, partName);
        }
        else
        {
            Debug.LogError("[AvatarPartNetworkSync] Cannot set part because AvatarPartSetter is null.");
        }

        // 2) Create the message to be sent.
        var msg = new PartMessage() { category = category, partName = partName };
        string json = JsonUtility.ToJson(msg);
        Debug.Log($"[AvatarPartNetworkSync] Sending JSON message: {json}");

        // 3) Send the ephemeral JSON message to remote peers.
        context.SendJson(msg);

        // 4) Persist the change in the room state so new joiners can read it.
        roomClient.Room[avatarPartKey] = json;
    }

    private void ApplyPersistedCustomization()
    {
        var partData = roomClient.Room[avatarPartKey];
        if (!string.IsNullOrEmpty(partData))
        {
            var msg = JsonUtility.FromJson<PartMessage>(partData);
            if (msg != null && !string.IsNullOrEmpty(msg.category))
            {
                Debug.Log($"[AvatarPartNetworkSync] Applying persisted change on init: {msg.category} -> {msg.partName}");
                partSetter?.SetPart(msg.category, msg.partName);
            }
        }
    }

    [Serializable]
    private class TestMessage
    {
        public string text;
    }

    [Serializable]
    private class PartMessage
    {
        public string category;
        public string partName;
    }
}