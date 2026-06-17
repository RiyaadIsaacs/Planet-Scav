using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{
    public void LoadBeginnerLevel() => LoadGameplayScene("Beginner");

    public void LoadTutorialLevel() => LoadGameplayScene("Tutorial");

    public void LoadAdvancedLevel() => LoadGameplayScene("Advanced");

    public void LoadFinalLevel() => LoadGameplayScene("Final Level");

    public void QuitGame()
    {
        Application.Quit();
    }

    private static void LoadGameplayScene(string sceneName)
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(sceneName);
    }

    // Kept for any legacy UI hooks still pointing at old method names.
    public void PlayButton() => LoadBeginnerLevel();

    public void OnApplicationQuit() => QuitGame();

    public void firstSceneSelected() => LoadBeginnerLevel();

    public void secondSceneSelected() => LoadAdvancedLevel();
}
