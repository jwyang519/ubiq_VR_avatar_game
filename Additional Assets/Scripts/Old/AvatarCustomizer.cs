using System.Collections.Generic;
using UnityEngine;

public class AvatarCustomizer : MonoBehaviour
{
    private Dictionary<string, Dictionary<string, SkinnedMeshRenderer>> data = new Dictionary<string, Dictionary<string, SkinnedMeshRenderer>>();
    private Dictionary<string, SkinnedMeshRenderer> smr = new Dictionary<string, SkinnedMeshRenderer>();
    private string[,] avatarStr = new string[,] { {"Hair", "1"}, {"Top", "1"}, {"Bottom", "1"}, {"Shoes", "1"}, {"Face", "A1"} };

    private Transform sourceTrans;
    private GameObject target;
    private Transform[] hips;

    public void Initialize(Transform sourceTrans, GameObject target, Transform[] hips)
    {
        this.sourceTrans = sourceTrans;
        this.target = target;
        this.hips = hips;
        SaveData();
    }

    private void SaveData()
    {
        data.Clear();
        smr.Clear();

        if (sourceTrans == null)
            return;

        Transform partsParent = sourceTrans.Find("Parts");
        if (partsParent == null)
        {
            Debug.LogError("No 'Parts' folder found in source model!");
            return;
        }

        foreach (Transform category in partsParent)
        {
            if (!data.ContainsKey(category.name))
                data.Add(category.name, new Dictionary<string, SkinnedMeshRenderer>());

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

            foreach (Transform part in category)
            {
                SkinnedMeshRenderer partSMR = part.GetComponent<SkinnedMeshRenderer>();
                if (partSMR != null)
                {
                    data[category.name].Add(part.name, partSMR);
                    Debug.Log("Collected part: " + part.name + " under category: " + category.name);
                }
            }

            if (!smr.ContainsKey(category.name))
            {
                GameObject partGo = new GameObject(category.name);
                partGo.transform.parent = targetCategory;
                smr.Add(category.name, partGo.AddComponent<SkinnedMeshRenderer>());
            }
        }
    }

    public void ChangeMesh(string part, string num)
    {
        string fullPartName = part + "_" + num;

        if (!data.ContainsKey(part) || !data[part].ContainsKey(fullPartName))
        {
            Debug.LogError("No part found with key " + fullPartName);
            return;
        }

        SkinnedMeshRenderer skm = data[part][fullPartName];

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

        smr[part].bones = bones.ToArray();
        smr[part].materials = skm.materials;
        smr[part].sharedMesh = skm.sharedMesh;

        SaveDataForPart(part, num);
    }

    private void SaveDataForPart(string part, string num)
    {
        int length = avatarStr.GetLength(0);
        for (int i = 0; i < length; i++)
        {
            if (avatarStr[i, 0] == part)
            {
                avatarStr[i, 1] = num;
            }
        }
    }

    public void InitializeDefaultParts()
    {
        int length = avatarStr.GetLength(0);
        for (int i = 0; i < length; i++)
        {
            ChangeMesh(avatarStr[i, 0], avatarStr[i, 1]);
        }
    }
} 