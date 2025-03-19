using UnityEngine;

[RequireComponent(typeof(Transform))]
public class AvatarInitializer : MonoBehaviour
{
    [SerializeField] private GameObject modelPrefab;
    [SerializeField] private GameObject targetPrefab;

    private Transform sourceTrans;
    private GameObject target;
    private Transform[] hips;

    public Transform SourceTrans => sourceTrans;
    public GameObject Target => target;
    public Transform[] Hips => hips;

    private void Awake()
    {
        // Ensure we have the required components
        if (modelPrefab == null || targetPrefab == null)
        {
            Debug.LogWarning("Model or Target prefab not assigned in AvatarInitializer!");
        }
    }

    public void InitializeAvatar()
    {
        if (modelPrefab == null || targetPrefab == null)
        {
            Debug.LogError("Please assign the model and target prefabs in the Unity Inspector!");
            return;
        }

        // Initialize the resource model (hidden)
        GameObject go = Instantiate(modelPrefab);
        sourceTrans = go.transform;
        go.SetActive(false);

        // Initialize the target avatar with physics
        target = Instantiate(targetPrefab);
        SetupPhysics();
        CalculateVolumeAndSetupCollider();
        
        // Get all bone transforms for the avatar
        hips = target.GetComponentsInChildren<Transform>();
    }

    private void SetupPhysics()
    {
        // Set basic transform properties
        target.transform.position = new Vector3(2, 1.5f, 2);
        target.transform.rotation = Quaternion.identity;
        target.transform.localScale = new Vector3(1, 1, 1);
        
        // Setup Rigidbody
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = target.AddComponent<Rigidbody>();
        }
        
        // Configure Rigidbody properties
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.mass = 70f;
        rb.linearDamping = 1f;
        rb.angularDamping = 0.05f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | 
                        RigidbodyConstraints.FreezeRotationY | 
                        RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void CalculateVolumeAndSetupCollider()
    {
        // Get ALL types of renderers (SkinnedMeshRenderer and MeshRenderer)
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true); // true to include inactive objects
        SkinnedMeshRenderer[] skinnedRenderers = target.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        
        if (renderers.Length == 0 && skinnedRenderers.Length == 0)
        {
            Debug.LogError("No renderers found in the avatar!");
            return;
        }

        // Initialize bounds
        Bounds bounds = new Bounds();
        bool boundsInitialized = false;

        // Include regular renderers
        foreach (Renderer renderer in renderers)
        {
            if (!boundsInitialized)
            {
                bounds = renderer.bounds;
                boundsInitialized = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        // Include skinned mesh renderers
        foreach (SkinnedMeshRenderer renderer in skinnedRenderers)
        {
            if (!boundsInitialized)
            {
                bounds = renderer.bounds;
                boundsInitialized = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }

            // Also check bones to ensure we catch extremities
            Transform[] bones = renderer.bones;
            foreach (Transform bone in bones)
            {
                if (bone != null)
                {
                    bounds.Encapsulate(bone.position);
                }
            }
        }

        // Calculate dimensions
        float height = bounds.size.y;
        float radius = Mathf.Max(bounds.size.x, bounds.size.z) / 2f;
        float volume = Mathf.PI * radius * radius * height;

        // Log the measurements
        Debug.Log($"Avatar Measurements (Including all parts):");
        Debug.Log($"Height: {height:F2} units");
        Debug.Log($"Width: {bounds.size.x:F2} units");
        Debug.Log($"Depth: {bounds.size.z:F2} units");
        Debug.Log($"Approximate Volume: {volume:F2} cubic units");
        Debug.Log($"Center point: {bounds.center}");
        Debug.Log($"Bounds extents: {bounds.extents}");
    }
} 