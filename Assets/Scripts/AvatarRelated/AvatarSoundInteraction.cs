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
            if (avatar.IsLocal) return;
            
            var interactor = args.interactorObject;
            if (interactor != null)
            {
                PlayInteractionSound(interactor.transform.name);
            }
        }

        private void OnInteractableGrabbed(SelectEnterEventArgs args)
        {
            if (avatar.IsLocal) return;
            
            var interactor = args.interactorObject;
            if (interactor != null)
            {
                PlayInteractionSound(interactor.transform.name);
            }
        }

        private void PlayInteractionSound(string interactorId)
        {
            if (avatar.IsLocal) return;
            
            // Send network message
            if (context.Scene != null)
            {
                context.SendJson(new InteractionMessage { interactorId = interactorId });
            }
            
            // Play sound locally
            if (audioSource && interactionSound)
            {
                audioSource.PlayOneShot(interactionSound);
            }
        }

        /// <summary>
        /// Receive and process interaction messages from the network
        /// </summary>
        public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
        {
            var interactionMessage = message.FromJson<InteractionMessage>();
            
            if (audioSource && interactionSound)
            {
                audioSource.PlayOneShot(interactionSound);
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