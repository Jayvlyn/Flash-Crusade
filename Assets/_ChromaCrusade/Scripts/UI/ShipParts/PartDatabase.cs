using System.Collections.Generic;
using UnityEngine;
using static ImporterPart;

public class PartDatabase : MonoBehaviour
{
    public static PartDatabase Instance { get; private set; }

    public ShipPartList LoadedList { get; private set; }

    private readonly Dictionary<string, ShipPartData> lookup = new();

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadNamesFromJson();
        LoadAssets();
    }

    private void LoadNamesFromJson()
    {
        TextAsset json = Resources.Load<TextAsset>("PartList");
        if (json == null)
        {
            Debug.LogError("PartList.json missing! Generate it from Tools > Generate Ship Part List");
            return;
        }

        LoadedList = JsonUtility.FromJson<ShipPartList>(json.text);
    }

    private void LoadAssets()
    {
        var allParts = Resources.LoadAll<ShipPartData>("Parts");

        foreach (var part in allParts)
        {
            if (!lookup.ContainsKey(part.name))
                lookup.Add(part.name, part);
            else
                Debug.LogWarning($"Duplicate part name detected: {part.name}");
        }

        Debug.Log($"PartDatabase initialized with {lookup.Count} parts.");
    }

    public ShipPartData Get(string partName)
    {
        if (lookup.TryGetValue(partName, out var value))
            return value;

        Debug.LogError($"Missing part: {partName}");
        return null;
    }

    public ShipPartData[] GetPartsOfType(PartType type)
    {
        if (LoadedList == null)
        {
            Debug.LogError("PartDatabase has not loaded or PartList.json is missing.");
            return System.Array.Empty<ShipPartData>();
        }

        List<string> targetNames = type switch
        {
            PartType.Cabin => LoadedList.cabins,
            PartType.Core => LoadedList.cores,
            PartType.Utility => LoadedList.utilities,
            PartType.Wing => LoadedList.wings,
            PartType.Weapon => LoadedList.weapons,
            _ => null
        };

        if (targetNames == null)
        {
            Debug.LogError($"Unhandled part type: {type}");
            return System.Array.Empty<ShipPartData>();
        }

        List<ShipPartData> results = new();

        foreach (var name in targetNames)
        {
            if (lookup.TryGetValue(name, out var part))
                results.Add(part);
            else
                Debug.LogWarning($"Part '{name}' in JSON list for '{type}' not found in lookup.");
        }

        return results.ToArray();
    }
}
