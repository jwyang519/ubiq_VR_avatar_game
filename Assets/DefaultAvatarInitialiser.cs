using UnityEngine;

public class DefaultAvatarInitialiser : MonoBehaviour
{
    // Define the default parts to apply upon spawn.
    // Each row: { category, defaultVariant }.
    // You can adjust these defaults as needed.
    private string[,] defaultParts = new string[,] {
        {"Hair", "1"},
        {"Top", "1"},
        {"Bottom", "1"},
        {"Shoes", "1"},
        {"Face", "A1"}
    };

    void Start()
    {
        // Wait one frame to ensure that other components (like AvatarCustomizer) have initialized.
        StartCoroutine(InitializeDefaults());
    }

    System.Collections.IEnumerator InitializeDefaults()
    {
        yield return null; // Wait one frame

        // Attempt to get the AvatarCustomizer component attached to the avatar.
        AvatarCustomizer customizer = GetComponent<AvatarCustomizer>();
        if (customizer == null)
        {
            Debug.LogError("DefaultAvatarInitializer: No AvatarCustomizer component found on this avatar!");
            yield break;
        }

        // Loop through each default part and apply it.
        int rows = defaultParts.GetLength(0);
        for (int i = 0; i < rows; i++)
        {
            string category = defaultParts[i, 0];
            string variant = defaultParts[i, 1];
            customizer.OnChangePart(category, variant);
            Debug.Log($"DefaultAvatarInitializer: Set {category} to variant {variant}");
        }
    }
}