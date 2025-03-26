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

    [Tooltip("Interpolation factor for smoothing remote updates.")]
    [Range(0f, 1f)]
    public float interpolationFactor = 0.1f;

    private Vector3 lastSentPosition;
    private Quaternion lastSentRotation;

    // Target transform values received from the network.
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    // Flags to control sync and physics.
    private bool localGrabbed = false;   // True when this client is holding the object.
    private bool remoteGrabbed = false;  // Set from network messages.

    private Rigidbody rb;

    void Start()
    {
        // Register with the NetworkScene (make sure one exists in your scene).
        context = NetworkScene.Register(this);
        lastSentPosition = transform.position;
        lastSentRotation = transform.rotation;
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Only send updates if this client is not holding the object.
        if (!localGrabbed)
        {
            if (Vector3.Distance(transform.position, lastSentPosition) > positionThreshold ||
                Quaternion.Angle(transform.rotation, lastSentRotation) > rotationThreshold)
            {
                lastSentPosition = transform.position;
                lastSentRotation = transform.rotation;
                SendState();
            }
        }

        // Smoothly interpolate the object's transform toward the target (from network messages).
        transform.position = Vector3.Lerp(transform.position, targetPosition, interpolationFactor);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, interpolationFactor);

        // On remote clients, if the object is marked as grabbed, disable physics so gravity doesn't act.
        if (rb != null)
        {
            if (remoteGrabbed)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            else if (!localGrabbed) // Only re-enable physics when not locally grabbed.
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }
    }

    void SendState()
    {
        StateMessage msg = new StateMessage()
        {
            position = transform.position,
            rotation = transform.rotation,
            grabbed = localGrabbed
        };
        context.SendJson(msg);
    }

    // This method is called by Ubiq when a network message is received.
    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        StateMessage msg = message.FromJson<StateMessage>();
        targetPosition = msg.position;
        targetRotation = msg.rotation;
        remoteGrabbed = msg.grabbed;

        // Update last sent values so we don't immediately trigger another update.
        lastSentPosition = msg.position;
        lastSentRotation = msg.rotation;
    }

    [System.Serializable]
    private struct StateMessage
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool grabbed;
    }

    // --- These methods should be hooked up to your XR Grab Interactable events. ---
    // Call this when the object is grabbed.
    public void OnSelectEntered()
    {
        localGrabbed = true;
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        SendState(); // Broadcast that the object is now grabbed.
    }

    // Call this when the object is released.
    public void OnSelectExited()
    {
        localGrabbed = false;
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        // Optionally delay syncing to let the object settle.
        StartCoroutine(ResumeSyncAfterDelay(0.5f));
    }

    private IEnumerator ResumeSyncAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        lastSentPosition = transform.position;
        lastSentRotation = transform.rotation;
        SendState();
    }
}