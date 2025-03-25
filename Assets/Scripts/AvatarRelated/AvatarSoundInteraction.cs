using Ubiq.Avatars;
using Ubiq.Messaging;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Ubiq.Samples
{
    /// <summary>
    /// Component that allows an avatar to emit sounds when interacted with by other players in VR
    /// </summary>
    public class AvatarSoundInteraction : MonoBehaviour
    {
        [Header("Audio Settings")]
        [Tooltip("Sound to play when interacted with by another player")]
        public AudioClip interactionSound;
        
        [Header("Debug Settings")]
        [Tooltip("Show collider in runtime")]
        public bool showCollider = true;
        
        [Tooltip("Color of the collider visualization")]
        public Color colliderColor = new Color(1f, 0f, 0f, 0.3f); // Red with transparency
        
        // Components
        private AudioSource audioSource;
        private Ubiq.Avatars.Avatar avatar;
        private NetworkContext context;
        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;
        private Collider objectCollider;
        private Material debugMaterial;

        private void Start()
        {
            // Get the Avatar component from parent (set by AvatarManager)
            avatar = GetComponentInParent<Ubiq.Avatars.Avatar>();
            if (!avatar)
            {
                Debug.LogWarning($"[{gameObject.name}] Could not find Avatar component!");
                enabled = false;
                return;
            }

            // Check for existing collider
            objectCollider = GetComponent<Collider>();
            if (!objectCollider)
            {
                Debug.LogWarning($"[{gameObject.name}] No collider found! Please add a collider to the object for interaction.");
                enabled = false;
                return;
            }
            Debug.Log($"[{gameObject.name}] Using existing collider: {objectCollider.GetType().Name}");

            // Create debug material if needed
            if (showCollider)
            {
                CreateDebugMaterial();
            }

            // Setup audio source
            audioSource = GetComponent<AudioSource>();
            if (!audioSource)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1.0f; // Make sound 3D
                audioSource.playOnAwake = false;
            }

            // Setup interactable
            interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
            if (!interactable)
            {
                Debug.Log($"[{gameObject.name}] Adding XRSimpleInteractable component");
                interactable = gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
                // Configure interactable to work with trigger colliders
                interactable.interactionLayers = InteractionLayerMask.GetMask("Default");
                interactable.colliders.Clear();
                interactable.colliders.Add(objectCollider);
                interactable.selectEntered.AddListener(OnInteractableGrabbed);
                
                // Additional settings for trigger colliders
                interactable.allowTriggerColliders = true;
                interactable.requireCollider = true;
                interactable.requireHover = true;
                interactable.requireSelect = true;
            }
            else
            {
                // If interactable already exists, ensure it's configured for trigger colliders
                interactable.colliders.Clear();
                interactable.colliders.Add(objectCollider);
                interactable.allowTriggerColliders = true;
            }

            // Register for network messages
            context = NetworkScene.Register(this, NetworkId.Create(avatar.NetworkId, "sound_interaction"));
        }

        private void CreateDebugMaterial()
        {
            // Create a new material for visualization
            debugMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            debugMaterial.color = colliderColor;
            debugMaterial.SetFloat("_Surface", 1); // Transparent
            debugMaterial.SetFloat("_Blend", 0); // Alpha
            debugMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            debugMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            debugMaterial.SetInt("_ZWrite", 0);
            debugMaterial.DisableKeyword("_ALPHATEST_ON");
            debugMaterial.EnableKeyword("_ALPHABLEND_ON");
            debugMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            debugMaterial.renderQueue = 3000;
        }

        private void OnDrawGizmos()
        {
            if (!showCollider || !objectCollider) return;

            // Draw the collider
            Gizmos.color = colliderColor;
            if (objectCollider is BoxCollider boxCollider)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCollider.center, boxCollider.size);
            }
            else if (objectCollider is SphereCollider sphereCollider)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius);
            }
            else if (objectCollider is CapsuleCollider capsuleCollider)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Vector3 size = Vector3.zero;
                switch (capsuleCollider.direction)
                {
                    case 0: // X-axis
                        size = new Vector3(capsuleCollider.height, capsuleCollider.radius * 2, capsuleCollider.radius * 2);
                        break;
                    case 1: // Y-axis
                        size = new Vector3(capsuleCollider.radius * 2, capsuleCollider.height, capsuleCollider.radius * 2);
                        break;
                    case 2: // Z-axis
                        size = new Vector3(capsuleCollider.radius * 2, capsuleCollider.radius * 2, capsuleCollider.height);
                        break;
                }
                Gizmos.DrawCube(capsuleCollider.center, size);
            }
        }

        private void OnDestroy()
        {
            if (interactable)
            {
                interactable.selectEntered.RemoveListener(OnInteractableGrabbed);
            }
            if (debugMaterial)
            {
                Destroy(debugMaterial);
            }
        }

        private void OnInteractableGrabbed(SelectEnterEventArgs args)
        {
            Debug.Log($"[AvatarSoundInteraction] Grab interaction triggered by: {args.interactorObject?.transform.name}");
            var interactor = args.interactorObject;
            if (interactor != null)
            {
                PlayInteractionSound(interactor.transform.name);
            }
        }

        private void PlayInteractionSound(string interactorId)
        {
            Debug.Log($"[AvatarSoundInteraction] Attempting to play sound. AudioSource exists: {audioSource != null}, Sound clip exists: {interactionSound != null}");
            
            // Send network message
            if (context.Scene != null)
            {
                context.SendJson(new InteractionMessage { interactorId = interactorId });
                Debug.Log("[AvatarSoundInteraction] Network message sent");
            }
            
            // Play sound locally
            if (audioSource && interactionSound)
            {
                audioSource.PlayOneShot(interactionSound);
                Debug.Log("[AvatarSoundInteraction] Sound played successfully");
            }
            else
            {
                Debug.LogWarning("[AvatarSoundInteraction] Failed to play sound - missing AudioSource or sound clip");
            }
        }

        /// <summary>
        /// Receive and process interaction messages from the network
        /// </summary>
        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            Debug.Log("[AvatarSoundInteraction] Received network message");
            var interactionMessage = message.FromJson<InteractionMessage>();
            
            if (audioSource && interactionSound)
            {
                audioSource.PlayOneShot(interactionSound);
                Debug.Log("[AvatarSoundInteraction] Network-triggered sound played successfully");
            }
            else
            {
                Debug.LogWarning("[AvatarSoundInteraction] Failed to play network-triggered sound - missing AudioSource or sound clip");
            }
        }

        /// <summary>
        /// Structure for network interaction messages
        /// </summary>
        private struct InteractionMessage
        {
            public string interactorId;
        }
    }
} 