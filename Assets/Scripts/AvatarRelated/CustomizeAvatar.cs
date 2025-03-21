using Ubiq.Avatars;
using UnityEngine;

namespace Ubiq.Samples
{
    /// <summary>
    /// Simple avatar with only face functionality, designed for Face_A5
    /// </summary>
    public class CustomizeAvatar : MonoBehaviour
    {
        public Transform head;
        public Renderer headRenderer;

        // Optional components - we'll try to find them but won't require them
        [Header("Optional Components")]
        public HeadAndHandsAvatar headAndHandsAvatar;
        public TexturedAvatar texturedAvatar;

        private InputVar<Pose> lastGoodHeadPose;
        private bool isSetup = false;

        private void Awake()
        {
            // Check for AvatarManager in the scene
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
            // If components are not directly assigned, try to find them
            if (!head)
            {
                head = transform;
                Debug.Log($"[{gameObject.name}] Using self as head transform");
            }

            if (!headRenderer)
            {
                headRenderer = GetComponent<Renderer>();
                if (!headRenderer)
                {
                    Debug.LogWarning($"[{gameObject.name}] No renderer found on Face_A5!");
                }
                else
                {
                    Debug.Log($"[{gameObject.name}] Found renderer component");
                }
            }

            if (!headAndHandsAvatar)
            {
                headAndHandsAvatar = GetComponentInParent<HeadAndHandsAvatar>();
                if (!headAndHandsAvatar)
                {
                    Debug.Log($"[{gameObject.name}] Working in standalone mode without HeadAndHandsAvatar");
                }
                else
                {
                    Debug.Log($"[{gameObject.name}] Found HeadAndHandsAvatar component");
                }
            }

            if (!texturedAvatar)
            {
                texturedAvatar = GetComponentInParent<TexturedAvatar>();
                if (!texturedAvatar)
                {
                    Debug.Log($"[{gameObject.name}] Working in standalone mode without TexturedAvatar");
                }
                else
                {
                    Debug.Log($"[{gameObject.name}] Found TexturedAvatar component");
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

            if (texturedAvatar)
            {
                texturedAvatar.OnTextureChanged.AddListener(TexturedAvatar_OnTextureChanged);
                Debug.Log($"[{gameObject.name}] Subscribed to texture updates");
            }
        }

        private void OnDisable()
        {
            if (headAndHandsAvatar)
            {
                headAndHandsAvatar.OnHeadUpdate.RemoveListener(HeadAndHandsEvents_OnHeadUpdate);
            }

            if (texturedAvatar)
            {
                texturedAvatar.OnTextureChanged.RemoveListener(TexturedAvatar_OnTextureChanged);
            }
        }

        private void HeadAndHandsEvents_OnHeadUpdate(InputVar<Pose> pose)
        {
            if (!headRenderer) return;

            if (!pose.valid)
            {
                if (!lastGoodHeadPose.valid)
                {
                    headRenderer.enabled = false;
                    return;
                }
                
                pose = lastGoodHeadPose;
            }
            
            if (head)
            {
                head.position = pose.value.position;
                head.rotation = pose.value.rotation;        
                lastGoodHeadPose = pose;
            }
        }

        private void TexturedAvatar_OnTextureChanged(Texture2D tex)
        {
            if (headRenderer && headRenderer.material && tex)
            {
                headRenderer.material.mainTexture = tex;
                Debug.Log($"[{gameObject.name}] Updated texture");
            }
        }

        // Optional: Add this method if you want to update the face position manually
        public void UpdateFaceTransform(Vector3 position, Quaternion rotation)
        {
            if (head)
            {
                head.position = position;
                head.rotation = rotation;
            }
        }
    }
}
