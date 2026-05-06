using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject optionsMenu;

    private bool isPaused;

    private void OnEnable()
    {
        EventHandler.OnPauseRequested += TogglePause;
    }

    private void OnDisable()
    {
        EventHandler.OnPauseRequested -= TogglePause;
    }

    // handling the pause toggle
    void TogglePause()
    {
        isPaused = !isPaused;

        pauseMenu.SetActive(isPaused);
        optionsMenu.SetActive(false); // always close options when toggling pause

        Time.timeScale = isPaused ? 0f : 1f;

        //// Cursor control 
        //Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        //Cursor.visible = isPaused;
    }

    // Button Functions for UI

    public void ResumeGame()
    {
        isPaused = false;

        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);

        Time.timeScale = 1f;

        //Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenOptions()
    {
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void LoadMainMenu()
    {
        // Unpause before switching scenes
        Time.timeScale = 1f;
        isPaused = false;

        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");

        Application.Quit();

        // Trying this for editor testing
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}