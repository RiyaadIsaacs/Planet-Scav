using UnityEngine;

public class MainMenuButtons : MonoBehaviour
{ 
    public void PlayButton()
    {
               UnityEngine.SceneManagement.SceneManager.LoadScene("Beginner");
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }
}
