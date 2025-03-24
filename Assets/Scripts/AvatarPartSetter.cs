using UnityEngine;

public class AvatarPartSetter : MonoBehaviour
{
    [Tooltip("Name of the container holding all parts. Defaults to 'Parts'.")]
    public string partsContainerName = "Parts";

    /// <summary>
    /// Sets a new part for the specified category by enabling the corresponding child.
    /// This version expects the partName (e.g., "Headgear_4") as a string.
    /// </summary>
    /// <param name="category">The part category (e.g., 'Headgear').</param>
    /// <param name="partName">The name of the part to enable.</param>
    public void SetPart(string category, string partName)
    {
        Transform partsContainer = transform.Find(partsContainerName);
        if (partsContainer == null)
        {
            Debug.LogError("AvatarPartSetter: Parts container '" + partsContainerName + "' not found on avatar.");
            return;
        }

        Transform categoryTransform = partsContainer.Find(category);
        if (categoryTransform == null)
        {
            Debug.LogError("AvatarPartSetter: Category '" + category + "' not found under parts container.");
            return;
        }

        // Disable all current parts in this category.
        foreach (Transform child in categoryTransform)
        {
            child.gameObject.SetActive(false);
        }

        // Find the child with the given name and enable it.
        Transform chosenPart = categoryTransform.Find(partName);
        if (chosenPart)
        {
            chosenPart.gameObject.SetActive(true);
            Debug.Log("AvatarPartSetter: Enabled " + partName + " for category " + category);
        }
        else
        {
            Debug.LogError("AvatarPartSetter: Could not find a part named '" + partName + "' in category " + category);
        }
    }
}