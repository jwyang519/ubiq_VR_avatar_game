using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Ubiq.Avatars; // For AvatarManager

public class AvatarCustomizationUI : MonoBehaviour
{
    [Tooltip("Prefab for the UI Button (must include a Button component and a label, e.g., TextMeshProUGUI).")]
    public GameObject buttonPrefab;

    [Tooltip("Parent container (e.g., the Content object of a Scroll View) for generated buttons.")]
    public Transform buttonContainer;

    [Tooltip("The category to customize (e.g., 'Shoes', 'Top'). Must match the name of a child in the avatar’s parts container.")]
    public string categoryName;

    [Tooltip("Reference to the AvatarManager that instantiates the local avatar.")]
    public AvatarManager avatarManager;

    private List<GameObject> customizationParts = new List<GameObject>();

    private List<Button> spawnedButtons = new List<Button>();

    private AvatarPartSetter partSetter;

    private GameObject lastLocalAvatar;

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

        StartCoroutine(WaitForLocalAvatarAndGenerateButtons());
    }

    private void Update()
    {
        if (avatarManager.LocalAvatar != null)
        {
            if (avatarManager.LocalAvatar.gameObject != lastLocalAvatar)
            {
                lastLocalAvatar = avatarManager.LocalAvatar.gameObject;
                RefreshUI();
            }
        }
    }

    private void RefreshUI()
    {
        if (avatarManager.LocalAvatar == null ||
            avatarManager.LocalAvatar.GetComponent<AvatarPartNetworkSync>() == null)
        {
            buttonContainer.gameObject.SetActive(false);
        }
        else
        {
            buttonContainer.gameObject.SetActive(true);
            StartCoroutine(WaitForLocalAvatarAndGenerateButtons());
        }
    }

    private IEnumerator WaitForLocalAvatarAndGenerateButtons()
    {
        while (avatarManager.LocalAvatar == null)
        {
            yield return null;
        }

        partSetter = avatarManager.LocalAvatar.GetComponent<AvatarPartSetter>();
        if (partSetter == null)
        {
            Debug.LogError("AvatarCustomizationUI: Local avatar does not have an AvatarPartSetter component.");
            yield break;
        }

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

        customizationParts.Clear();
        foreach (Transform child in categoryContainer)
        {
            customizationParts.Add(child.gameObject);
        }

        GenerateButtons();
    }

    private void GenerateButtons()
    {
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedButtons.Clear();


        GameObject resetButtonObj = Instantiate(buttonPrefab, buttonContainer);
        Button resetButton = resetButtonObj.GetComponent<Button>();
        if (resetButton == null)
        {
            Debug.LogError("AvatarCustomizationUI: The button prefab is missing a Button component.");
        }
        else
        {
            spawnedButtons.Add(resetButton);

            // Set the label to RESET
            TextMeshProUGUI tmpLabel = resetButtonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpLabel != null)
            {
                tmpLabel.text = "Reset";
            }
            else
            {
                Text uiText = resetButtonObj.GetComponentInChildren<Text>();
                if (uiText != null)
                {
                    uiText.text = "Reset";
                }
            }

            resetButton.onClick.AddListener(() =>
            {
                AvatarPartNetworkSync netSync = avatarManager.LocalAvatar.GetComponent<AvatarPartNetworkSync>();
                if (netSync)
                {
                    netSync.SetPartNetworked(categoryName, null);
                }

                foreach (Button btn in spawnedButtons) { btn.interactable = true; }
                resetButton.interactable = false;
            });
        }

        for (int i = 0; i < customizationParts.Count; i++)
        {
            GameObject partOption = customizationParts[i];

            GameObject newButtonObj = Instantiate(buttonPrefab, buttonContainer);
            Button button = newButtonObj.GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError("AvatarCustomizationUI: The button prefab is missing a Button component.");
                continue;
            }
            spawnedButtons.Add(button);

            TextMeshProUGUI tmpLabel = newButtonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpLabel != null)
            {
                tmpLabel.text = FormatPartName(partOption.name);
            }
            else
            {
                Text uiText = newButtonObj.GetComponentInChildren<Text>();
                if (uiText != null)
                {
                    uiText.text = partOption.name;
                }
            }

            button.onClick.AddListener(() =>
            {
                AvatarPartNetworkSync netSync = avatarManager.LocalAvatar.GetComponent<AvatarPartNetworkSync>();
                if (netSync)
                {
                    netSync.SetPartNetworked(categoryName, partOption.name);
                }
                else
                {
                    Debug.LogWarning("Current avatar is not customizable (AvatarPartNetworkSync component missing).");
                    return;
                }

                foreach (Button btn in spawnedButtons) { btn.interactable = true; }
                button.interactable = false;
            });
        }
    }

    private string FormatPartName(string rawName)
    {
        string withSpaces = rawName.Replace("_", " ");

        if (withSpaces.Length > 0)
        {
            withSpaces = char.ToUpper(withSpaces[0]) + withSpaces.Substring(1);
        }

        return withSpaces;
    }
}