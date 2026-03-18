using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject newSaveMenu;
    [SerializeField] UINavigator nav;
    [SerializeField] NavItem nameFactionInput;
    [SerializeField] PlayerSaveManager saveManager;

    public void OnNewGamePressed()
    {
        EditorState.context = EditorContext.StartGame;

        newSaveMenu.SetActive(true);
        mainMenu.SetActive(false);

        NavState.PrevScreenItem = NavState.currentItem;
        nav.NavToItem(nameFactionInput);
  
        saveManager.LoadSaveNames();
    }

    public void OnLoadGamePressed()
    {
        EditorState.context = EditorContext.MidGame;
        //SceneManager.LoadScene("Scene_Game");
        //SceneManager.LoadScene("Scene_Builder"); // temp
    }

    public void OnCreativePressed()
    {
        EditorState.context = EditorContext.Creative;
        SceneManager.LoadScene("Scene_Builder");
    }

    public void OnOptionsPressed()
    {
        Debug.Log("options pressed (incomplete)");
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }


    // New Save Menu Buttons

    public void OnCancelNewSave()
    {
        newSaveMenu.SetActive(false);
        mainMenu.SetActive(true);
        nav.NavToItem(NavState.PrevScreenItem);
    }

    public void OnStartNewSave()
    {
        EditorState.context = EditorContext.StartGame;
        SceneManager.LoadScene("Scene_Builder"); // build first ship
    }
}
