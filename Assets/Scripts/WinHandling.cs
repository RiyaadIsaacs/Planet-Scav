using UnityEngine;

public class WinHandling : MonoBehaviour
{
    public Canvas winCanvas;

    private void OnEnable()
    {
        EventHandler.OnBossDefeated += ShowWinScreen;
    }

    private void OnDisable()
    {
        EventHandler.OnBossDefeated -= ShowWinScreen;
    }

    public void ShowWinScreen()
    {
        if (winCanvas == null)
        {
            var found = GameObject.Find("WinCanvas");
            if (found != null)
                winCanvas = found.GetComponent<Canvas>();
        }

        if (winCanvas == null)
        {
            Debug.LogError("WinHandling: WinCanvas reference is missing.");
            return;
        }

        winCanvas.gameObject.SetActive(true);
        Time.timeScale = 0f;
        SFXManager.PauseMusic();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        var dialogueUI = FindFirstObjectByType<DialogueUIManager>();
        if (dialogueUI != null)
            dialogueUI.SetHudOverlayActive(false);
    }
}
