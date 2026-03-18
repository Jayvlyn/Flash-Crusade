using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerSaveManager : MonoBehaviour
{
    public HashSet<string> saveNames;

    public void LoadSaveNames()
    {
        Directory.CreateDirectory(Paths.PlayerSavesPath);

        saveNames = new();

        string[] files = Directory.GetFiles(Paths.PlayerSavesPath, "*.json", SearchOption.TopDirectoryOnly);
        foreach (string file in files)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            saveNames.Add(name);
        }
    }
}
