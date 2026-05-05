using UnityEngine;

public class MainMenuButtons : MonoBehaviour
{
    public string selectedScene;

    public void sceneSelectButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Scene Select");
    }

    public void PlayButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(selectedScene);
    }

    public void MainMenuButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }

    public void firstSceneSelected()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Beginner");
        selectedScene = "Beginner";
    }

    public void secondSceneSelected()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Advanced");
        selectedScene = "Advanced";
    }
}
