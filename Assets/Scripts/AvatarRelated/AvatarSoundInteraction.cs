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
        
        // Components
        private AudioSource audioSource;
        private Ubiq.Avatars.Avatar avatar;
        private NetworkContext context;
        private UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable interactable;

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
            var collider = GetComponent<Collider>();
            if (!collider)
            {
                Debug.LogWarning($"[{gameObject.name}] No collider found! Please add a collider to the object for interaction.");
                enabled = false;
                return;
            }
            Debug.Log($"[{gameObject.name}] Using existing collider: {collider.GetType().Name}");

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
                interactable.hoverEntered.AddListener(OnInteractableHovered);
                interactable.selectEntered.AddListener(OnInteractableGrabbed);
            }

            // Register for network messages
            context = NetworkScene.Register(this, NetworkId.Create(avatar.NetworkId, "sound_interaction"));
        }

        private void OnDestroy()
        {
            if (interactable)
            {
                interactable.hoverEntered.RemoveListener(OnInteractableHovered);
                interactable.selectEntered.RemoveListener(OnInteractableGrabbed);
            }
        }

        private void OnInteractableHovered(HoverEnterEventArgs args)
        {
            Debug.Log($"[AvatarSoundInteraction] Hover interaction triggered by: {args.interactorObject?.transform.name}");
            var interactor = args.interactorObject;
            if (interactor != null)
            {
                PlayInteractionSound(interactor.transform.name);
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