using System.Collections;
using UnityEngine;
using Ubiq.Avatars;

namespace Ubiq.Examples
{
    public class AvatarPrefabSetterNew : MonoBehaviour
    {
        // Reference to the AvatarManager on the same GameObject
        private AvatarManager manager;

        private void Awake()
        {
            manager = GetComponent<AvatarManager>();
            if (manager == null)
            {
                Debug.LogError("AvatarManager not found on this GameObject.");
            }
        }

        public void Set(int index)
        {
            // 1. Save the current customization data.
            var currentSync = manager.LocalAvatar ? manager.LocalAvatar.GetComponent<AvatarPartNetworkSync>() : null;
            AvatarCustomizationData savedData = new AvatarCustomizationData();
            if (currentSync != null)
            {
                savedData = currentSync.GetLocalCustomization();
            }

            // 2. Set the new prefab (this will eventually force a respawn to the base model)
            manager.avatarPrefab = manager.avatarCatalogue.prefabs[index];

            // 3. Rely on AvatarManager's Update loop to trigger a respawn, so don't call UpdateLocalAvatar() explicitly.

            // 4. Start a coroutine to wait for the new avatar and then reapply the customization.
            StartCoroutine(ReapplyCustomizationAfterSpawn(savedData));
        }

        private IEnumerator ReapplyCustomizationAfterSpawn(AvatarCustomizationData data)
        {
            // Wait until the local avatar is re-spawned.
            while (manager.LocalAvatar == null)
            {
                yield return null;
            }

            // Give one frame for initialization (optional).
            yield return null;

            // Get the AvatarPartNetworkSync on the new avatar.
            var sync = manager.LocalAvatar?.GetComponent<AvatarPartNetworkSync>();
            if (sync == null)
            {
                Debug.LogWarning("Current avatar is not customizable. The AvatarPartNetworkSync component is missing.");
                yield break;
            }

            // Reapply each customization from the saved data.
            for (int i = 0; i < data.categories.Count; i++)
            {
                string category = data.categories[i];
                string partName = data.partNames[i];
                if (!string.IsNullOrEmpty(category) && !string.IsNullOrEmpty(partName))
                {
                    // Use your network sync method to update both locally and in the room.
                    sync.SetPartNetworked(category, partName);
                }
            }

            // Optionally, refresh your UI so that the button references update.
            // For example, trigger a UI refresh event if needed.
        }
    }
}