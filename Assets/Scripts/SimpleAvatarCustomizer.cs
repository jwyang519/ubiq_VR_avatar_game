using System.Collections.Generic;
using UnityEngine;

public class SimpleAvatarCustomizer : MonoBehaviour
{
    public Transform partsRoot;
    private Dictionary<string, List<GameObject>> partVariants = new Dictionary<string, List<GameObject>>();
    public bool IsReady { get; private set; } = false;

    void Awake()
    {
        if (partsRoot == null)
        {
            Debug.LogError("Parts root not assigned!");
            return;
        }

        foreach (Transform category in partsRoot)
        {
            string categoryName = category.name;
            partVariants[categoryName] = new List<GameObject>();

            foreach (Transform variant in category)
            {
                partVariants[categoryName].Add(variant.gameObject);
                variant.gameObject.SetActive(false);
            }
        }

        IsReady = true;
    }

    public void ShowVariant(string category, string variantName)
    {
        if (!partVariants.ContainsKey(category))
        {
            Debug.LogWarning($"Category '{category}' not found!");
            return;
        }

        foreach (var variant in partVariants[category])
        {
            variant.SetActive(variant.name == variantName);
        }

        Debug.Log($"[Avatar] Activated {variantName} in {category}");
    }

    public List<GameObject> GetPartVariants(string category)
    {
        if (!partVariants.ContainsKey(category))
            return null;

        return new List<GameObject>(partVariants[category]);
    }
}