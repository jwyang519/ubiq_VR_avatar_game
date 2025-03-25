using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class NetworkObject : MonoBehaviour, INetworkSpawnable
{
    private NetworkSpawnManager spawnManager;
    public NetworkId NetworkId { get; set; }
    public bool owner;

    [HideInInspector] public bool isGrabbed;
    
    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        owner = false;
    }

    private void Start()
    {
        // Use NetworkSpawnManager instead of directly registering NetworkedBehaviours
        spawnManager = NetworkSpawnManager.Find(this);
        
        // Setup VR interaction
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        owner = true;
        isGrabbed = true;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        owner = false;
        isGrabbed = false;
    }

    private void OnDestroy()
    {
        if (grabInteractable)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnRelease);
        }
    }
}