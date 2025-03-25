using System;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Messaging;
using UnityEngine.InputSystem;
using Ubiq.Avatars;
using Ubiq.Rooms; // Needed for accessing RoomClient


[Serializable]
public class AvatarCustomizationData
{
    public List<string> categories = new List<string>();
    public List<string> partNames = new List<string>();

    /// <summary>
    /// Update or add a part for the specified category.
    /// </summary>
    public void SetPart(string category, string partName)
    {
        int index = categories.IndexOf(category);
        if (index >= 0)
        {
            partNames[index] = partName;
        }
        else
        {
            categories.Add(category);
            partNames.Add(partName);
        }
    }

    /// <summary>
    /// Retrieve the part for the specified category.
    /// </summary>
    public string GetPart(string category)
    {
        int index = categories.IndexOf(category);
        return (index >= 0) ? partNames[index] : null;
    }
}

public class AvatarPartNetworkSync : MonoBehaviour
{
    private NetworkContext context;
    private AvatarPartSetter partSetter;
    private RoomClient roomClient; // Reference to the RoomClient
    private string avatarPartKey;  // Unique key to store avatar customization

    private float lastPingTime = 0f;
    private float pingCooldown = 2f; // seconds

    // Cache the local customization so we can push it even if we're not in a room yet.
    private AvatarCustomizationData localCustomization = new AvatarCustomizationData();

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

        // When joining a room, push our local customization (if any) into the room.
        roomClient.OnJoinedRoom.AddListener(room =>
        {
            string current = roomClient.Room[avatarPartKey];
            if (string.IsNullOrEmpty(current))
            {
                var json = JsonUtility.ToJson(localCustomization);
                roomClient.Room[avatarPartKey] = json;
                Debug.Log($"[AvatarPartNetworkSync] Pushed local customization on join: {json}");
            }
        });

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
        // Check for the "P" key to send a ping (example)
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

    // Updated ProcessMessage to handle AvatarCustomizationData messages.
    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        Debug.Log($"[AvatarPartNetworkSync] ProcessMessage called on {gameObject.name}");
        string rawMessage = message.ToString();
        Debug.Log($"[AvatarPartNetworkSync] Received raw message: {rawMessage}");

        try
        {
            AvatarCustomizationData remoteData = message.FromJson<AvatarCustomizationData>();
            if (remoteData != null && remoteData.categories != null && remoteData.categories.Count > 0)
            {
                Debug.Log($"[AvatarPartNetworkSync] Received remote customization data with {remoteData.categories.Count} entries.");
                ApplyRemoteCustomization(remoteData);
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[AvatarPartNetworkSync] Failed to parse customization data: " + e);
        }

        // Fallback if message isn't parsed.
        Debug.LogError("[AvatarPartNetworkSync] Failed to parse incoming message.");
    }

    private void ApplyRemoteCustomization(AvatarCustomizationData data)
    {
        for (int i = 0; i < data.categories.Count; i++)
        {
            string category = data.categories[i];
            string partName = data.partNames[i];
            Debug.Log($"[AvatarPartNetworkSync] Applying remote customization: {category} -> {partName}");
            partSetter?.SetPart(category, partName);
        }
    }

    // Called when the room state updates.
    private void OnRoomUpdated(IRoom room)
    {
        var json = room[avatarPartKey];
        if (!string.IsNullOrEmpty(json))
        {
            Debug.Log($"[AvatarPartNetworkSync] OnRoomUpdated found customization JSON: {json}");
            ApplyPersistedCustomization();
        }
    }

    // Apply the customization stored in the room.
    private void ApplyPersistedCustomization()
    {
        var json = roomClient.Room[avatarPartKey];
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                AvatarCustomizationData data = JsonUtility.FromJson<AvatarCustomizationData>(json);
                for (int i = 0; i < data.categories.Count; i++)
                {
                    string category = data.categories[i];
                    string partName = data.partNames[i];
                    if (!string.IsNullOrEmpty(category) && !string.IsNullOrEmpty(partName))
                    {
                        Debug.Log($"[AvatarPartNetworkSync] Applying persisted change: {category} -> {partName}");
                        partSetter?.SetPart(category, partName);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[AvatarPartNetworkSync] Failed to parse customization data: " + e);
            }
        }
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
            return;
        }

        // 2) Update our local cache.
        localCustomization.SetPart(category, partName);

        // 3) Serialize the full customization data.
        string json = JsonUtility.ToJson(localCustomization);
        Debug.Log($"[AvatarPartNetworkSync] Sending updated customization JSON: {json}");

        // 4) Optionally, send an ephemeral message to remote peers.
        context.SendJson(localCustomization);

        // 5) Persist the change in the room state so new joiners can read it.
        roomClient.Room[avatarPartKey] = json;
    }

    [Serializable]
    private class TestMessage
    {
        public string text;
    }
}