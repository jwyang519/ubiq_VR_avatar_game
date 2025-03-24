using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ubiq.Avatars; // For AvatarManager

/// <summary>
/// This script auto-generates UI buttons for customizing a specific avatar category (e.g., Shoes).
/// It automatically finds the local avatar (via AvatarManager), then searches its parts hierarchy.
/// When a button is clicked, it calls the AvatarPartSetter component (attached on the avatar)
/// to update that part. It also toggles button interactability so that the selected button is disabled.
/// </summary>
public class AvatarCustomizationUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Prefab for the UI Button (must include a Button component and a label, e.g., TextMeshProUGUI).")]
    public GameObject buttonPrefab;

    [Tooltip("Parent container (e.g., the Content object of a Scroll View) for generated buttons.")]
    public Transform buttonContainer;

    [Header("Customization Settings")]
    [Tooltip("The category to customize (e.g., 'Shoes', 'Top'). Must match the name of a child in the avatar’s parts container.")]
    public string categoryName;

    [Header("Avatar Manager")]
    [Tooltip("Reference to the AvatarManager that instantiates the local avatar.")]
    public AvatarManager avatarManager;

    // Internal list of part options found under the specified category.
    private List<GameObject> customizationParts = new List<GameObject>();

    // List of generated buttons (for toggling interactability).
    private List<Button> spawnedButtons = new List<Button>();

    // Cached reference to the AvatarPartSetter on the local avatar.
    private AvatarPartSetter partSetter;

    private void Start()
    {
        if (avatarManager == null)
        {
            Debug.LogError("AvatarCustomizationUI: AvatarManager is not assigned.");
            return;
        }
        if (buttonPrefab == null)
        {
            Debug.LogError("AvatarCustomizationUI: Button prefab is not assigned.");
            return;
        }
        if (buttonContainer == null)
        {
            Debug.LogError("AvatarCustomizationUI: Button container is not assigned.");
            return;
        }

        // Wait until the local avatar is instantiated.
        StartCoroutine(WaitForLocalAvatar());
    }

    private IEnumerator WaitForLocalAvatar()
    {
        while (avatarManager.LocalAvatar == null)
        {
            yield return null;
        }

        // Once available, get the AvatarPartSetter component.
        partSetter = avatarManager.LocalAvatar.GetComponent<AvatarPartSetter>();
        if (partSetter == null)
        {
            Debug.LogError("AvatarCustomizationUI: Local avatar does not have an AvatarPartSetter component.");
            yield break;
        }

        // Look for the parts container on the local avatar.
        // This example assumes the local avatar has a child named "Parts",
        // and under that a child with the name matching categoryName.
        Transform partsContainer = avatarManager.LocalAvatar.transform.Find("Parts");
        if (partsContainer == null)
        {
            Debug.LogError("AvatarCustomizationUI: Local avatar does not contain a 'Parts' container.");
            yield break;
        }

        Transform categoryContainer = partsContainer.Find(categoryName);
        if (categoryContainer == null)
        {
            Debug.LogError("AvatarCustomizationUI: Category '" + categoryName + "' not found under the Parts container.");
            yield break;
        }

        // Collect all available parts (each child is an option).
        customizationParts.Clear();
        foreach (Transform child in categoryContainer)
        {
            customizationParts.Add(child.gameObject);
        }

        GenerateButtons();
    }

    /// <summary>
    /// Instantiates a UI button for each available customization part and wires up its OnClick events.
    /// </summary>
    private void GenerateButtons()
    {
        // Clear any existing buttons from the container.
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedButtons.Clear();

        for (int i = 0; i < customizationParts.Count; i++)
        {
            int index = i; // local copy for the closure
            GameObject partOption = customizationParts[index];

            // Instantiate the button prefab under the button container.
            GameObject newButtonObj = Instantiate(buttonPrefab, buttonContainer);
            Button button = newButtonObj.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError("AvatarCustomizationUI: The button prefab is missing a Button component.");
                continue;
            }
            spawnedButtons.Add(button);

            // Set the button's label to the name of the part.
            TextMeshProUGUI tmpLabel = newButtonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpLabel != null)
            {
                tmpLabel.text = partOption.name;
            }
            else
            {
                Text uiText = newButtonObj.GetComponentInChildren<Text>();
                if (uiText != null)
                {
                    uiText.text = partOption.name;
                }
            }

            // Wire up the onClick event.
            button.onClick.AddListener(() =>
            {
                // 1) Find your AvatarPartNetworkSync
                AvatarPartNetworkSync netSync = avatarManager.LocalAvatar.GetComponent<AvatarPartNetworkSync>();
                if (netSync)
                {
                    netSync.SetPartNetworked(categoryName, partOption.name);
                }

                // 2) Toggle button states
                foreach (Button btn in spawnedButtons) { btn.interactable = true; }
                button.interactable = false;
            });
        }
    }
}