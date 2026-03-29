using UnityEngine;

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
        player.health = 3;
        deathHandling.HPGainHandler(); // Restore health visuals
        deathCanvas.gameObject.SetActive(false); // Hide the death canvas
        Time.timeScale = 1f; // Resume the game
    }

    public void MainMenu()
    {
        Time.timeScale= 1f; // Ensure the game is not paused when returning to the main menu
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
