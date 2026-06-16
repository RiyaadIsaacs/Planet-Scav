using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Lives on the persistent player and wires scene-specific references after each level loads.
/// </summary>
[DefaultExecutionOrder(-50)]
public class LevelReferenceBinder : MonoBehaviour
{
    [Header("Scene Object Names")]
    [SerializeField] private string playerCanvasName = "PlayerCanvas";
    [SerializeField] private string deathCanvasName = "DeathCanvas";
    [SerializeField] private string gameManagerName = "Game Manager";
    [SerializeField] private string cameraPivotName = "Camera Pivot";

    private PlayerController playerController;
    private PlayerStats playerStats;
    private PlayerCheckpoints playerCheckpoints;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();
        playerCheckpoints = GetComponent<PlayerCheckpoints>();
    }

    public void BindSceneReferences()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();
        if (playerCheckpoints == null)
            playerCheckpoints = GetComponent<PlayerCheckpoints>();

        var playerCanvasGo = FindSceneObjectByName(playerCanvasName);
        var deathCanvasGo = FindSceneObjectByName(deathCanvasName);
        var gameManagerGo = FindSceneObjectByName(gameManagerName);

        if (playerCanvasGo == null)
            Debug.LogWarning($"LevelReferenceBinder: '{playerCanvasName}' not found in scene.");
        if (deathCanvasGo == null)
            Debug.LogWarning($"LevelReferenceBinder: '{deathCanvasName}' not found in scene.");
        if (gameManagerGo == null)
            Debug.LogWarning($"LevelReferenceBinder: '{gameManagerName}' not found in scene.");

        var deathCanvas = deathCanvasGo != null ? deathCanvasGo.GetComponent<Canvas>() : null;
        var deathHandling = gameManagerGo != null ? gameManagerGo.GetComponent<DeathHandling>() : null;
        var uiButtons = gameManagerGo != null ? gameManagerGo.GetComponent<UIButtons>() : null;
        var uiManager = gameManagerGo != null ? gameManagerGo.GetComponent<UIManager>() : null;
        var dialogueUI = playerCanvasGo != null
            ? playerCanvasGo.GetComponent<DialogueUIManager>()
            : FindFirstObjectByType<DialogueUIManager>();

        var cameraPivot = transform.Find(cameraPivotName);
        var mainCamera = cameraPivot != null ? cameraPivot.GetComponentInChildren<Camera>() : Camera.main;

        BindPlayerController(playerController, deathHandling, cameraPivot, mainCamera);
        BindPlayerStats(playerStats, playerCanvasGo);
        BindPlayerCheckpoints(playerCheckpoints);
        BindDeathHandling(deathHandling, playerStats, playerCheckpoints, deathCanvas, playerCanvasGo);
        BindUIButtons(uiButtons, playerStats, deathHandling, deathCanvas);
        BindDeathCanvasButtons(deathCanvasGo, uiButtons);
        BindUIManager(uiManager, playerCanvasGo, dialogueUI);
        BindDialogueUI(dialogueUI, playerController);

        ResetPlayerForLevel(playerStats, deathHandling, deathCanvasGo);
    }

    private static void BindPlayerController(PlayerController controller, DeathHandling deathHandling,
        Transform cameraPivot, Camera mainCamera)
    {
        if (controller == null)
            return;

        controller.deathHandling = deathHandling;

        if (cameraPivot != null)
            controller.BindCameraPivot(cameraPivot);

        var playerInput = controller.GetComponent<PlayerInput>();
        if (playerInput != null && mainCamera != null)
            playerInput.camera = mainCamera;
    }

    private static void BindPlayerStats(PlayerStats stats, GameObject playerCanvasGo)
    {
        if (stats == null)
            return;

        stats.playerTrans = stats.transform;

        if (playerCanvasGo == null)
            return;

        var moneyText = playerCanvasGo.transform.Find("MoneyText")?.GetComponent<TMP_Text>();
        if (moneyText != null)
            stats.coinText = moneyText;
    }

    private void BindPlayerCheckpoints(PlayerCheckpoints checkpoints)
    {
        if (checkpoints == null)
            return;

        var spawn = GameObject.FindGameObjectWithTag("Spawn");
        if (spawn != null)
            checkpoints.SetSpawnPoint(spawn.transform);
        else
            Debug.LogWarning("LevelReferenceBinder: no object tagged 'Spawn' in scene.");
    }

    private static void BindDeathHandling(DeathHandling deathHandling, PlayerStats stats,
        PlayerCheckpoints checkpoints, Canvas deathCanvas, GameObject playerCanvasGo)
    {
        if (deathHandling == null)
            return;

        deathHandling.player = stats;
        deathHandling.checkPoints = checkpoints;
        if (deathCanvas != null)
            deathHandling.deathCanvas = deathCanvas;

        if (playerCanvasGo == null)
            return;

        deathHandling.healthThird = playerCanvasGo.transform.Find("HPRawImage (2)")?.GetComponent<RawImage>();
        deathHandling.healthSecond = playerCanvasGo.transform.Find("HPRawImage (1)")?.GetComponent<RawImage>();
        deathHandling.healthFirst = playerCanvasGo.transform.Find("HPRawImage")?.GetComponent<RawImage>();
    }

    private static void BindUIButtons(UIButtons uiButtons, PlayerStats stats, DeathHandling deathHandling,
        Canvas deathCanvas)
    {
        if (uiButtons == null)
            return;

        uiButtons.player = stats;
        uiButtons.deathHandling = deathHandling;
        if (deathCanvas != null)
            uiButtons.deathCanvas = deathCanvas;
    }

    private static void BindDeathCanvasButtons(GameObject deathCanvasGo, UIButtons uiButtons)
    {
        if (deathCanvasGo == null || uiButtons == null)
            return;

        WireRuntimeButton(FindButtonByName(deathCanvasGo, "RetryButton"), uiButtons.retry);
        WireRuntimeButton(FindButtonByName(deathCanvasGo, "MainMenuButton"), uiButtons.MainMenu);
        WireRuntimeButton(FindButtonByName(deathCanvasGo, "QuitButton"), uiButtons.QuitApplication);
    }

    private static void ResetPlayerForLevel(PlayerStats stats, DeathHandling deathHandling, GameObject deathCanvasGo)
    {
        if (stats == null)
            return;

        Time.timeScale = 1f;
        AudioListener.pause = false;
        stats.health = 3;

        if (deathCanvasGo != null)
            deathCanvasGo.SetActive(false);

        if (deathHandling != null)
            deathHandling.HPGainHandler();
    }

    private static Button FindButtonByName(GameObject root, string namePart)
    {
        foreach (var button in root.GetComponentsInChildren<Button>(true))
        {
            if (button.gameObject.name.Contains(namePart))
                return button;
        }

        return null;
    }

    private static void WireRuntimeButton(Button button, UnityAction action)
    {
        if (button == null || action == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private static void BindUIManager(UIManager uiManager, GameObject playerCanvasGo, DialogueUIManager dialogueUI)
    {
        if (uiManager == null || playerCanvasGo == null)
            return;

        uiManager.BindSceneUI(
            playerCanvasGo.transform.Find("PauseMenu")?.gameObject,
            playerCanvasGo.transform.Find("OptionsMenu")?.gameObject,
            dialogueUI);
    }

    private static void BindDialogueUI(DialogueUIManager dialogueUI, PlayerController controller)
    {
        if (dialogueUI == null)
            return;

        var levelConfig = FindFirstObjectByType<LevelConfig>();
        if (levelConfig != null && levelConfig.dialogueSequence != null)
            dialogueUI.ConfigureForLevel(controller, levelConfig.dialogueSequence, levelConfig.localizationFile);
        else
            dialogueUI.ConfigureForLevel(controller, dialogueUI.sequence, dialogueUI.localizationFile);
    }

    private static GameObject FindSceneObjectByName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
            return null;

        var activeMatch = GameObject.Find(objectName);
        if (activeMatch != null)
            return activeMatch;

        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            var match = FindChildByNameIncludingInactive(root.transform, objectName);
            if (match != null)
                return match.gameObject;
        }

        return null;
    }

    private static Transform FindChildByNameIncludingInactive(Transform parent, string objectName)
    {
        if (parent.name == objectName)
            return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            var match = FindChildByNameIncludingInactive(parent.GetChild(i), objectName);
            if (match != null)
                return match;
        }

        return null;
    }
}
