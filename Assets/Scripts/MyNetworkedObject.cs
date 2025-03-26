using System.Collections;
using UnityEngine;
using Ubiq.Messaging;

public class MyNetworkedObject : MonoBehaviour
{
    private NetworkContext context;

    [Tooltip("Only send position updates if the object has moved more than this (in meters).")]
    public float positionThreshold = 0.01f;

    [Tooltip("Only send rotation updates if the object has rotated more than this (in degrees).")]
    public float rotationThreshold = 1f;

    [Tooltip("Interpolation factor for smoothing remote updates (try a higher value like 0.3 for faster catch-up).")]
    [Range(0f, 1f)]
    public float interpolationFactor = 0.3f; // increased for faster convergence

    private Vector3 lastSentPosition;
    private Quaternion lastSentRotation;

    // Target transform values received from the network.
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    // Flag to indicate that the object is grabbed locally.
    private bool isGrabbed = false;

    private Rigidbody rb;

    private void Start()
    {
        // Register with the NetworkScene (ensure one exists in the scene).
        context = NetworkScene.Register(this);

        // Initialize positions and rotations
        lastSentPosition = transform.position;
        lastSentRotation = transform.rotation;
        targetPosition = transform.position;
        targetRotation = transform.rotation;

        // If a Rigidbody is attached, set initial physics properties
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Start with physics enabled and gravity on
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    private void Update()
    {
        // Only send updates when the object is grabbed locally.
        if (isGrabbed)
        {
            if (Vector3.Distance(transform.position, lastSentPosition) > positionThreshold ||
                Quaternion.Angle(transform.rotation, lastSentRotation) > rotationThreshold)
            {
                lastSentPosition = transform.position;
                lastSentRotation = transform.rotation;
                // Create a message with the world position and rotation.
                Message msg = new Message()
                {
                    position = transform.position,
                    rotation = transform.rotation
                };
                context.SendJson(msg);
            }

            // Only interpolate position when grabbed
            transform.position = Vector3.Lerp(transform.position, targetPosition, interpolationFactor);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, interpolationFactor);
        }
    }

    // Called when a network message is received.
    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Only process network messages if grabbed
        if (isGrabbed)
        {
            Message msg = message.FromJson<Message>();
            // Update target values so the object interpolates smoothly.
            targetPosition = msg.position;
            targetRotation = msg.rotation;
            // Also update last sent values so we don't immediately trigger another update.
            lastSentPosition = msg.position;
            lastSentRotation = msg.rotation;
        }
    }

    // Structure for the sync message.
    [System.Serializable]
    private struct Message
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    // These methods should be hooked up to XR Grab Interactable events.

    // Call this when the object is grabbed.
    public void OnSelectEntered()
    {
        isGrabbed = true;
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    // Call this when the object is released.
    public void OnSelectExited()
    {
        isGrabbed = false;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}