using UnityEngine;
using UnityEngine.XR;

public class VRMovementController : MonoBehaviour
{
    public Transform xrRig; // The root of your VR rig
    private Vector3 lastPosition;

    void Start()
    {
        // Initialize lastPosition with the current XR rig position.
        lastPosition = xrRig.position;
    }

    void Update()
    {
        // Calculate how much the XR rig has moved since the last frame.
        float distanceMoved = Vector3.Distance(xrRig.position, lastPosition);
        float speed = distanceMoved / Time.deltaTime;

        // Update the 'Speed' parameter on the avatar's Animator.
        if (AvatarSys._instance != null && AvatarSys._instance.CurrentAnimator != null)
        {
            AvatarSys._instance.CurrentAnimator.SetFloat("Speed", speed);
        }
        else
        {
            Debug.LogWarning("AvatarSys or its CurrentAnimator is not available yet.");
        }

        // Save the current position for the next frame.
        lastPosition = xrRig.position;
    }
}