using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Ubiq.Geometry;

public class NetworkObject : MonoBehaviour, INetworkSpawnable
{
    private NetworkSpawnManager spawnManager;
    private NetworkContext context;
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
        context = NetworkScene.Register(this);
        
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

    private void FixedUpdate()
    {
        if (owner)
        {
            SendMessage();
        }
    }

    private struct Message
    {
        public Pose pose;
        public bool isGrabbed;
    }

    private void SendMessage()
    {
        var message = new Message();
        message.pose = Transforms.ToLocal(transform, context.Scene.transform);
        message.isGrabbed = isGrabbed;
        context.SendJson(message);
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        var msg = message.FromJson<Message>();
        var pose = Transforms.ToWorld(msg.pose, context.Scene.transform);
        transform.position = pose.position;
        transform.rotation = pose.rotation;
        isGrabbed = msg.isGrabbed;
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