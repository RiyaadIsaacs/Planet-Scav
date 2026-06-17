using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtons : MonoBehaviour
{
    public PlayerStats player;
    public DeathHandling deathHandling;
    public Canvas deathCanvas;
    public void QuitApplication()
    {
        Application.Quit();
    }

    public void retry()
    {
        if (player == null || deathHandling == null || deathCanvas == null)
        {
            Debug.LogError("UIButtons: missing references for retry.");
            Time.timeScale = 1f;
            AudioListener.pause = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        player.health = 3;
        deathHandling.HPGainHandler(); // Restore health visuals
        deathCanvas.gameObject.SetActive(false); // Hide the death canvas
        Time.timeScale = 1f; // Resume the game
        AudioListener.pause = false;
        SFXManager.ResumeMusic();

        // Reload the currently active scene to reset everything in that scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SFXManager.ResumeMusic();
        SceneManager.LoadScene("MainMenu");
    }
}
