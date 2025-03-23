using Ubiq.Avatars;
using UnityEngine;

namespace Ubiq.Samples
{
    /// <summary>
    /// Avatar controller for QuickRig character root position
    /// </summary>
    public class AvatarControlUbiq : MonoBehaviour
    {
        [Header("Character Root")]
        public Transform characterRoot;  // QuickRigCharacter2_Reference

        [Header("Optional Components")]
        public HeadAndHandsAvatar headAndHandsAvatar;

        [Header("Rig Bones")]
        public Transform headBone;

        private InputVar<Pose> lastGoodPose;
        private bool isSetup = false;

        private void Awake()
        {
            var avatarManager = FindObjectOfType<AvatarManager>();
            if (!avatarManager)
            {
                Debug.LogError("No AvatarManager found in the scene! Please add one to the scene.");
            }
            else
            {
                Debug.Log("Found AvatarManager in scene");
            }
        }

        private void Start()
        {
            SetupComponents();
        }

        private void SetupComponents()
        {
            // Try to find character root if not assigned
            if (!characterRoot)
            {
                characterRoot = transform.Find("Bone/QuickRigCharacter2_Reference");
                if (characterRoot)
                {
                    Debug.Log($"[{gameObject.name}] Found character root reference");
                }
                else
                {
                    Debug.LogWarning($"[{gameObject.name}] Could not find QuickRigCharacter2_Reference!");
                }
            }

            // Find optional components
            if (!headAndHandsAvatar)
            {
                headAndHandsAvatar = GetComponentInParent<HeadAndHandsAvatar>();
                if (!headAndHandsAvatar)
                {
                    Debug.Log($"[{gameObject.name}] Working in standalone mode without HeadAndHandsAvatar");
                }
            }

            isSetup = true;
            Debug.Log($"[{gameObject.name}] Setup completed");
        }

        private void OnEnable()
        {
            if (!isSetup)
            {
                SetupComponents();
            }

            if (headAndHandsAvatar)
            {
                headAndHandsAvatar.OnHeadUpdate.AddListener(HeadAndHandsEvents_OnHeadUpdate);
                Debug.Log($"[{gameObject.name}] Subscribed to head updates");
            }
        }

        private void OnDisable()
        {
            if (headAndHandsAvatar)
            {
                headAndHandsAvatar.OnHeadUpdate.RemoveListener(HeadAndHandsEvents_OnHeadUpdate);
            }
        }

        private void HeadAndHandsEvents_OnHeadUpdate(InputVar<Pose> pose)
        {
            if (!characterRoot) return;

            if (!pose.valid)
            {
                if (!lastGoodPose.valid)
                {
                    return;
                }

                pose = lastGoodPose;
            }

            // Move body position (XZ only)
            Vector3 newPos = pose.value.position;
            newPos.y = characterRoot.position.y;
            characterRoot.position = newPos;

            // Rotate body on Y axis only
            Vector3 flatForward = Vector3.ProjectOnPlane(pose.value.forward, Vector3.up).normalized;
            if (flatForward.sqrMagnitude > 0.001f)
            {
                characterRoot.forward = flatForward;
            }

            // Rotate head with full pose (pitch/yaw/roll), relative to body
            if (headBone)
            {
                Quaternion bodyRotation = characterRoot.rotation;
                Quaternion headRotation = Quaternion.Inverse(bodyRotation) * pose.value.rotation;

                // Extract Euler angles for testing
                Vector3 euler = headRotation.eulerAngles;

                // TEMP DEBUG: See what these values look like
                Debug.Log($"[HEAD] Euler from headset relative to body: {euler}");

                // REMAP axes depending on rig — try variations here:
                Vector3 remapped = new Vector3(
                    euler.x,    // Up/Down
                    0,          // We ignore roll for now
                    euler.y     // Left/Right (may be Z depending on rig)
                );

                // Apply rotation
                headBone.localRotation = Quaternion.Euler(remapped);
            }

            lastGoodPose = pose;
        }

        /// <summary>
        /// Update character position and rotation
        /// </summary>
        public void UpdateCharacterTransform(Vector3 position, Quaternion rotation)
        {
            if (characterRoot)
            {
                characterRoot.position = position;
                characterRoot.rotation = rotation;
                Debug.Log($"[{gameObject.name}] Updated character transform");
            }
        }

        /// <summary>
        /// Get current character position
        /// </summary>
        public Vector3 GetCharacterPosition()
        {
            return characterRoot ? characterRoot.position : Vector3.zero;
        }

        /// <summary>
        /// Get current character rotation
        /// </summary>
        public Quaternion GetCharacterRotation()
        {
            return characterRoot ? characterRoot.rotation : Quaternion.identity;
        }
    }
}

