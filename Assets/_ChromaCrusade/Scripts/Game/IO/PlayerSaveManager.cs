using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerSaveManager : MonoBehaviour
{
    public static PlayerSave ActiveSave;

    public static HashSet<string> SaveNames;

    public static void LoadSaveNames()
    {
        Directory.CreateDirectory(Paths.PlayerSavesPath);

        SaveNames = new();

        string[] files = Directory.GetFiles(Paths.PlayerSavesPath, "*.json", SearchOption.TopDirectoryOnly);
        foreach (string file in files)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            SaveNames.Add(name);
        }
    }

    public static void CreateNewSave(string name)
    {
        PlayerSave save = new();
        save.Init(name);
        ActiveSave = save;
        SaveToJson(save);
    }

    public static void SaveToJson(PlayerSave save)
    {
        string json = JsonUtility.ToJson(save, true);

        string path = Paths.PlayerSavePath(save.saveName);

        File.WriteAllText(path,json);
    }

}
