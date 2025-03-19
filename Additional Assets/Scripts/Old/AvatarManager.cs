using UnityEngine;

[RequireComponent(typeof(AvatarInitializer))]
[RequireComponent(typeof(AvatarCustomizer))]
public class AvatarManager : MonoBehaviour
{
    public static AvatarManager Instance { get; private set; }

    private AvatarInitializer initializer;
    private AvatarCustomizer customizer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Get the required components
        initializer = GetComponent<AvatarInitializer>();
        customizer = GetComponent<AvatarCustomizer>();
    }

    private void Start()
    {
        if (initializer == null || customizer == null)
        {
            Debug.LogError("Please assign the AvatarInitializer and AvatarCustomizer components in the Unity Inspector!");
            return;
        }

        InitializeAvatar();
    }

    private void InitializeAvatar()
    {
        initializer.InitializeAvatar();
        customizer.Initialize(
            initializer.SourceTrans,
            initializer.Target,
            initializer.Hips
        );
        customizer.InitializeDefaultParts();
    }

    public void ChangeAvatarPart(string part, string num)
    {
        customizer.ChangeMesh(part, num);
    }
} 