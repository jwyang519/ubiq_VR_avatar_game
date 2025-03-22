using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleCustomizationUI : MonoBehaviour
{
    public string categoryName; // e.g. "Hair"
    public GameObject buttonPrefab;
    public Transform buttonContainer;

    [HideInInspector] public List<GameObject> partPrefabs = new List<GameObject>();

    private SimpleAvatarCustomizer customizer;
    private bool buttonsGenerated = false;

    void Update()
    {
        if (!buttonsGenerated)
        {
            TryInit();
        }
    }

    void TryInit()
    {
        // Wait for the avatar to exist
        customizer = FindObjectOfType<SimpleAvatarCustomizer>();
        if (customizer == null || customizer.IsReady == false)
            return;

        Debug.Log($"[UI] Found customizer, populating parts for {categoryName}");

        var variants = customizer.GetPartVariants(categoryName);
        if (variants != null)
        {
            partPrefabs = variants;
            GenerateButtons();
            buttonsGenerated = true;
        }
        else
        {
            Debug.LogWarning($"[UI] No variants found for category: {categoryName}");
        }
    }

    void GenerateButtons()
    {
        Debug.Log($"{categoryName} has {partPrefabs.Count} prefabs.");
        foreach (GameObject partPrefab in partPrefabs)
        {
            GameObject newButton = Instantiate(buttonPrefab, buttonContainer);
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = partPrefab.name;


            newButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                customizer.ShowVariant(categoryName, partPrefab.name);
            });
        }

        Debug.Log($"[UI] Spawned {partPrefabs.Count} buttons for {categoryName}");
    }

    string ExtractID(string name)
    {
        string[] split = name.Split('_');
        return split.Length > 1 ? split[1] : name;
    }
}