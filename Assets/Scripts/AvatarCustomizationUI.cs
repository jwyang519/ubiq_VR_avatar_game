using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarCustomizationUI : MonoBehaviour
{
    [Header("Category Settings")]
    [Tooltip("Enter the category name exactly as used in AvatarSys (e.g., \"Hair\", \"Top\", \"Bottom\", \"Shoes\", \"Face\").")]
    public string category;

    [Header("UI References")]
    [Tooltip("The parent container where buttons will be added (e.g., the Content object of a Scroll View).")]
    public Transform buttonContainer;
    [Tooltip("A prefab for a UI Button that contains a Text component to display the variant name.")]
    public Button buttonPrefab;

    IEnumerator Start()
    {
        // Wait one frame to allow AvatarSys to finish initializing.
        yield return null;

        // Query available variants from AvatarSys.
        Dictionary<string, SkinnedMeshRenderer> parts;
        if (AvatarSys._instance != null && AvatarSys._instance.GetPartsForCategory(category, out parts))
        {
            Debug.Log("Found " + parts.Count + " parts for category: " + category);
            foreach (KeyValuePair<string, SkinnedMeshRenderer> kvp in parts)
            {
                // Instantiate a new button.
                Button newButton = Instantiate(buttonPrefab, buttonContainer);

                // Set the button's GameObject name to the key.
                newButton.name = kvp.Key;

                // Get the Text component on the button and update its text.
                Text btnText = newButton.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = kvp.Key;
                }
                else
                {
                    Debug.LogWarning("Button prefab is missing a Text component.");
                }

                // Extract the variant number (assuming key format "Category_Variant")
                string variantNumber = kvp.Key.Substring(category.Length + 1);
                string part = category;
                string variant = variantNumber;

                // Capture the current values for the lambda.
                string capturedPart = part;
                string capturedVariant = variant;

                newButton.onClick.AddListener(() =>
                {
                    // 1. Update the local avatar.
                    AvatarSys._instance.OnChangePart(capturedPart, capturedVariant);

                    // 2. Get the network sync component from the current avatar.
                    AvatarNetworkSync sync = AvatarSys._instance.CurrentAvatar.GetComponent<AvatarNetworkSync>();
                    if (sync != null)
                    {
                        // Broadcast the change over the network.
                        sync.SendAvatarChange(capturedPart, capturedVariant);
                    }
                    else
                    {
                        Debug.LogWarning("AvatarNetworkSync component is missing on the current avatar!");
                    }
                });
            }
        }
        else
        {
            Debug.LogWarning("No parts found for category: " + category);
        }
    }
}