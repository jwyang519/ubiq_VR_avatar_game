using UnityEngine;
using Ubiq.Avatars;

public class AvatarAnimationController : MonoBehaviour
{
    private Animator animator;
    private Ubiq.Avatars.Avatar avatar;
    private Vector3 lastPosition;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        avatar = GetComponent<Ubiq.Avatars.Avatar>(); // fully qualified to avoid ambiguity
        lastPosition = transform.position;
    }

    private void Update()
    {
        // Compute the movement delta regardless of local or remote
        Vector3 delta = transform.position - lastPosition;
        float speed = delta.magnitude / Time.deltaTime;

        if (animator != null)
        {
            animator.SetFloat("Speed", speed);
        }

        lastPosition = transform.position;
    }
}