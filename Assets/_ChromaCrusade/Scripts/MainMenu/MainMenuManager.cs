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
    }

    public void OnLoadGamePressed()
    {
        EditorState.context = EditorContext.MidGame;
        SceneManager.LoadScene("Scene_Game");
    }

    public void OnCreativePressed()
    {
        EditorState.context = EditorContext.Creative;
        SceneManager.LoadScene("Scene_Builder");
    }

    public void OnOptionsPressed()
    {

    }

    public void OnQuitPressed()
    {

    }
}
