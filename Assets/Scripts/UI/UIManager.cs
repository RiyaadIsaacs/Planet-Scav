using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private DialogueUIManager dialogueUI;

    private bool isPaused;

    private void Awake()
    {
        if (dialogueUI == null)
            dialogueUI = FindFirstObjectByType<DialogueUIManager>();
    }

    public void BindSceneUI(GameObject pause, GameObject options, DialogueUIManager dialogue)
    {
        if (pause != null)
            pauseMenu = pause;
        if (options != null)
            optionsMenu = options;
        if (dialogue != null)
            dialogueUI = dialogue;

        ResetPauseState();
    }

    private void ResetPauseState()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);
        if (optionsMenu != null)
            optionsMenu.SetActive(false);

        if (dialogueUI != null)
            dialogueUI.SetHudOverlayActive(true);

        SFXManager.ResumeMusic();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

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
        if (pauseMenu == null)
            return;

        isPaused = !isPaused;

        pauseMenu.SetActive(isPaused);
        optionsMenu.SetActive(false); // always close options when toggling pause

        if (dialogueUI != null)
            dialogueUI.SetHudOverlayActive(!isPaused);

        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
            SFXManager.PauseMusic();
        else
            SFXManager.ResumeMusic();

        // Cursor control 
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }

    // Button Functions for UI

    public void ResumeGame()
    {
        isPaused = false;

        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);

        if (dialogueUI != null)
            dialogueUI.SetHudOverlayActive(true);

        Time.timeScale = 1f;
        SFXManager.ResumeMusic();

        Cursor.lockState = CursorLockMode.Locked;
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

        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);

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