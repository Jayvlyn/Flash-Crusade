using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerSaveManager : MonoBehaviour
{
    HashSet<string> saveNames;

    public void LoadSaveNames()
    {
        saveNames = new();

        //string[] dataFiles = Directory.GetFiles(, "*.json", SearchOption.TopDirectoryOnly);
    }
}
