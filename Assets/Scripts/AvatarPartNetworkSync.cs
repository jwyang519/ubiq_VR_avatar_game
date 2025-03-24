using System;
using UnityEngine;
using Ubiq.Messaging; // This should contain NetworkContext and ReferenceCountedSceneGraphMessage
using UnityEngine.InputSystem;
using Ubiq.Avatars;

public class AvatarPartNetworkSync : MonoBehaviour
{
    private NetworkContext context;
    private AvatarPartSetter partSetter;
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
        // Register with the avatar's network id
        context = NetworkScene.Register(this, NetworkId.Create(avatar.NetworkId, nameof(AvatarPartNetworkSync)));
        Debug.Log($"[AvatarPartNetworkSync] Registered on network with id: {context.GetHashCode()}");

        partSetter = GetComponent<AvatarPartSetter>();
        if (partSetter == null)
        {
            Debug.LogError("[AvatarPartNetworkSync] AvatarPartSetter component not found on this avatar.");
        }
        else
        {
            Debug.Log("[AvatarPartNetworkSync] Found AvatarPartSetter component.");
        }
    }

    private void Update()
    {
        // Use the new Input System to check for the "P" key.
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

    public void SetPartNetworked(string category, string partName)
    {
        Debug.Log($"[AvatarPartNetworkSync] Local SetPartNetworked called with category: {category}, partName: {partName}");

        // Update locally using the AvatarPartSetter.
        if (partSetter != null)
        {
            partSetter.SetPart(category, partName);
        }
        else
        {
            Debug.LogError("[AvatarPartNetworkSync] Cannot set part because AvatarPartSetter is null.");
        }

        // Create a message and log its JSON representation.
        var msg = new PartMessage() { category = category, partName = partName };
        string json = JsonUtility.ToJson(msg);
        Debug.Log($"[AvatarPartNetworkSync] Sending JSON message: {json}");

        // Send the JSON message to remote peers.
        context.SendJson(msg);
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