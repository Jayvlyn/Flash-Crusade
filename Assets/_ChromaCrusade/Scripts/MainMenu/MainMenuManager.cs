using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void OnNewGamePressed()
    {
        EditorState.context = EditorContext.StartGame;
        //SceneManager.LoadScene("Scene_Opening");
        // then after opening scene:
        //SceneManager.LoadScene("Scene_Builder"); // build first ship
        SceneManager.LoadScene("Scene_Game"); // temp
    }

    public void OnLoadGamePressed()
    {
        EditorState.context = EditorContext.MidGame;
        //SceneManager.LoadScene("Scene_Game");
        SceneManager.LoadScene("Scene_Builder"); // temp
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
}
