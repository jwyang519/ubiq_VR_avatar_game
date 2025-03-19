using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarCustomizer : MonoBehaviour
{
    // This script should be attached to the NetworkedAvatar prefab.
    // It uses a hidden model repository (modelPrefab) to collect body-part variants,
    // and then lets you change parts via the OnChangePart method.

    [Header("Customization Settings")]
    [Tooltip("Hidden model repository containing all available parts (must have a 'Parts' folder).")]
    [SerializeField] private GameObject modelPrefab;

    // Data structures for storing parts info.
    private Transform sourceTrans;  // from the hidden repository
    private Dictionary<string, Dictionary<string, SkinnedMeshRenderer>> data = new Dictionary<string, Dictionary<string, SkinnedMeshRenderer>>();
    private Transform[] hips;
    private Dictionary<string, SkinnedMeshRenderer> smr = new Dictionary<string, SkinnedMeshRenderer>();

    // Default parts selection: { category, variant }
    private string[,] avatarStr = new string[,] {
        { "Hair", "1" },
        { "Top", "1" },
        { "Bottom", "1" },
        { "Shoes", "1" },
        { "Face", "A1" }
    };

    // For convenience, expose the current avatar (this GameObject) and its Animator.
    public GameObject CurrentAvatar => gameObject;
    public Animator CurrentAnimator { get; private set; }

    void Awake()
    {
        // Get the Animator component attached to the spawned avatar.
        CurrentAnimator = GetComponent<Animator>();
        if (CurrentAnimator == null)
        {
            Debug.LogError("No Animator component found on the avatar!");
        }
    }

    void Start()
    {
        // Instantiate the hidden model repository as a child of this avatar.
        if (modelPrefab == null)
        {
            Debug.LogError("Please assign the modelPrefab repository for customization parts!");
            return;
        }
        GameObject sourceGO = Instantiate(modelPrefab, transform.position, Quaternion.identity, transform);
        sourceTrans = sourceGO.transform;
        sourceGO.SetActive(false); // hide the repository

        // Get all transforms in the avatar (for remapping bones).
        hips = GetComponentsInChildren<Transform>();

        // Collect parts data from the hidden repository into the data and smr dictionaries.
        SaveData(sourceTrans, data, gameObject, smr);

        // Initialize the avatar with the default parts.
        InitAvatar();
    }

    // Scans the hidden repository for parts and sets up the target's parts structure.
    void SaveData(Transform sourceTrans, Dictionary<string, Dictionary<string, SkinnedMeshRenderer>> data, GameObject target, Dictionary<string, SkinnedMeshRenderer> smr)
    {
        data.Clear();
        smr.Clear();

        if (sourceTrans == null)
            return;

        // Look for the "Parts" folder in the source repository.
        Transform partsParent = sourceTrans.Find("Parts");
        if (partsParent == null)
        {
            Debug.LogError("No 'Parts' folder found in source model!");
            return;
        }

        // Iterate through each category (e.g., Hair, Top, etc.)
        foreach (Transform category in partsParent)
        {
            if (!data.ContainsKey(category.name))
                data.Add(category.name, new Dictionary<string, SkinnedMeshRenderer>());

            // Find (or create) the corresponding category folder in the target avatar.
            Transform targetPartsParent = target.transform.Find("Parts");
            if (targetPartsParent == null)
            {
                Debug.LogError("No 'Parts' folder found in target model!");
                continue;
            }
            Transform targetCategory = targetPartsParent.Find(category.name);
            if (targetCategory == null)
            {
                GameObject newCategory = new GameObject(category.name);
                newCategory.transform.parent = targetPartsParent;
                targetCategory = newCategory.transform;
            }

            // For each part in the category (e.g., Hair_1, Hair_2, etc.)
            foreach (Transform part in category)
            {
                SkinnedMeshRenderer partSMR = part.GetComponent<SkinnedMeshRenderer>();
                if (partSMR != null)
                {
                    // Use the naming convention: "Category_Variant" (e.g., "Hair_1").
                    data[category.name].Add(part.name, partSMR);
                    Debug.Log("Collected part: " + part.name + " under category: " + category.name);
                }
            }

            // Create an empty SkinnedMeshRenderer on the target for this category if not already created.
            if (!smr.ContainsKey(category.name))
            {
                GameObject partGo = new GameObject(category.name);
                partGo.transform.parent = targetCategory;
                smr.Add(category.name, partGo.AddComponent<SkinnedMeshRenderer>());
            }
        }
    }

    // Changes the target's mesh for a given category using the specified variant number.
    // 'part' is the category (e.g., "Hair") and 'num' is the variant (e.g., "1" for "Hair_1").
    void ChangeMesh(string part, string num, Dictionary<string, Dictionary<string, SkinnedMeshRenderer>> data, Transform[] hips, Dictionary<string, SkinnedMeshRenderer> smr, string[,] str)
    {
        string fullPartName = part + "_" + num;

        if (!data.ContainsKey(part) || !data[part].ContainsKey(fullPartName))
        {
            Debug.LogError("No part found with key " + fullPartName);
            return;
        }

        SkinnedMeshRenderer skm = data[part][fullPartName];

        // Clear existing mesh on this category (if any)
        if (smr.ContainsKey(part))
        {
            smr[part].sharedMesh = null;
            smr[part].materials = new Material[0];
        }

        // Remap bones from the source part to the target's bones.
        List<Transform> bones = new List<Transform>();
        foreach (var bone in skm.bones)
        {
            foreach (var targetBone in hips)
            {
                if (targetBone.name == bone.name)
                {
                    bones.Add(targetBone);
                    break;
                }
            }
        }
        // Replace the target's part with the new mesh, materials, and bones.
        smr[part].bones = bones.ToArray();
        smr[part].materials = skm.materials;
        smr[part].sharedMesh = skm.sharedMesh;

        SaveDataForPart(part, num, str);
    }

    // Updates the record for the given part in the default parts selection.
    void SaveDataForPart(string part, string num, string[,] str)
    {
        int length = str.GetLength(0);
        for (int i = 0; i < length; i++)
        {
            if (str[i, 0] == part)
            {
                str[i, 1] = num;
            }
        }
    }

    // Initializes the avatar using the default parts specified in avatarStr.
    void InitAvatar()
    {
        int length = avatarStr.GetLength(0);
        for (int i = 0; i < length; i++)
        {
            ChangeMesh(avatarStr[i, 0], avatarStr[i, 1], data, hips, smr, avatarStr);
        }
    }

    // Public method for UI to change a part on the fly.
    public void OnChangePart(string part, string num)
    {
        ChangeMesh(part, num, data, hips, smr, avatarStr);
        Debug.Log($"Changed {part} to variant {num}");
    }

    // Public method to expose available variants for a given category.
    public bool GetPartsForCategory(string category, out Dictionary<string, SkinnedMeshRenderer> parts)
    {
        if (data.ContainsKey(category))
        {
            parts = data[category];
            return true;
        }
        parts = null;
        return false;
    }
}