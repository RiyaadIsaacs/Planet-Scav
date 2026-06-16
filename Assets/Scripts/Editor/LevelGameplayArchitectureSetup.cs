#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class LevelGameplayRootAutoCreate
{
    private const string LevelRootPath = "Assets/Prefab/LevelGameplayRoot.prefab";
    private const string GameSessionConfigPath = "Assets/Resources/GameSessionConfig.asset";

    static LevelGameplayRootAutoCreate()
    {
        EditorApplication.delayCall += EnsureSetupAssetsExist;
    }

    private static void EnsureSetupAssetsExist()
    {
        EnsureGameSessionConfigExists();
        if (!File.Exists(LevelRootPath))
            LevelGameplayArchitectureSetup.CreateOrUpdateLevelGameplayRoot();
    }

    private static void EnsureGameSessionConfigExists()
    {
        LevelGameplayArchitectureSetup.EnsureGameSessionConfig();
    }
}

public static class LevelGameplayArchitectureSetup
{
    private const string PlayerPrefabPath = "Assets/Prefab/Player.prefab";
    private const string LevelRootPath = "Assets/Prefab/LevelGameplayRoot.prefab";
    private const string PlayerCanvasPath = "Assets/Prefab/UI Canvas/PlayerCanvas.prefab";
    private const string DeathCanvasPath = "Assets/Prefab/UI Canvas/DeathCanvas.prefab";

    [MenuItem("Planet Scav/Setup Level Gameplay Architecture")]
    public static void SetupAll()
    {
        EnsureGameSessionConfig();
        CreateOrUpdateLevelGameplayRoot();
        DecouplePlayerPrefab();
        AssetDatabase.SaveAssets();
        Debug.Log("Level gameplay architecture setup complete. Add LevelGameplayRoot + LevelConfig to each gameplay scene.");
    }

    [MenuItem("Planet Scav/Ensure GameSession Config")]
    public static void EnsureGameSessionConfig()
    {
        const string path = "Assets/Resources/GameSessionConfig.asset";
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (playerPrefab == null)
        {
            Debug.LogError($"Missing player prefab at {PlayerPrefabPath}");
            return;
        }

        var existing = AssetDatabase.LoadAssetAtPath<GameSessionConfig>(path);
        if (existing == null)
        {
            existing = ScriptableObject.CreateInstance<GameSessionConfig>();
            AssetDatabase.CreateAsset(existing, path);
        }

        existing.playerPrefab = playerPrefab;
        EditorUtility.SetDirty(existing);
        AssetDatabase.SaveAssets();
        Debug.Log($"GameSessionConfig updated at {path}");
    }

    [MenuItem("Planet Scav/Create LevelGameplayRoot Prefab")]
    public static void CreateOrUpdateLevelGameplayRoot()
    {
        var root = new GameObject("LevelGameplayRoot");
        root.AddComponent<LevelConfig>();

        var gameManager = new GameObject("Game Manager");
        gameManager.transform.SetParent(root.transform, false);
        gameManager.AddComponent<DeathHandling>();
        gameManager.AddComponent<UIButtons>();
        var uiManager = gameManager.AddComponent<UIManager>();

        var playerCanvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerCanvasPath);
        var deathCanvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DeathCanvasPath);

        GameObject playerCanvas = null;
        GameObject deathCanvas = null;

        if (playerCanvasPrefab != null)
        {
            playerCanvas = (GameObject)PrefabUtility.InstantiatePrefab(playerCanvasPrefab, root.transform);
            playerCanvas.name = "PlayerCanvas";
        }

        if (deathCanvasPrefab != null)
        {
            deathCanvas = (GameObject)PrefabUtility.InstantiatePrefab(deathCanvasPrefab, root.transform);
            deathCanvas.name = "DeathCanvas";
            deathCanvas.SetActive(false);

            var deathCanvasComponent = deathCanvas.GetComponent<Canvas>();
            var deathHandling = gameManager.GetComponent<DeathHandling>();
            var uiButtons = gameManager.GetComponent<UIButtons>();
            if (deathCanvasComponent != null && deathHandling != null)
                deathHandling.deathCanvas = deathCanvasComponent;
            if (deathCanvasComponent != null && uiButtons != null)
                uiButtons.deathCanvas = deathCanvasComponent;
        }

        if (playerCanvas != null)
            WireCanvasButtonsToUIManager(playerCanvas, uiManager);

        if (deathCanvas != null)
        {
            var uiButtons = gameManager.GetComponent<UIButtons>();
            if (uiButtons != null)
                WireDeathCanvasButtonsToUIButtons(deathCanvas, uiButtons);
        }

        PrefabUtility.SaveAsPrefabAsset(root, LevelRootPath);
        Object.DestroyImmediate(root);
        Debug.Log($"Saved {LevelRootPath}");
    }

    [MenuItem("Planet Scav/Decouple Player Prefab")]
    public static void DecouplePlayerPrefab()
    {
        var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        try
        {
            var playerTransform = FindPlayerTransform(root);
            if (playerTransform == null)
            {
                Debug.LogError("Could not find player transform in Player prefab.");
                return;
            }

            DestroyChildIfExists(playerTransform, "Game Manager");
            DestroyChildIfExists(playerTransform, "PlayerCanvas");
            DestroyChildIfExists(playerTransform, "DeathCanvas");

            if (root.GetComponent<LevelReferenceBinder>() == null)
                root.AddComponent<LevelReferenceBinder>();

            ClearBrokenReferences(root);
            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            Debug.Log("Player prefab decoupled — only character, camera, and persistent scripts remain.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static Transform FindPlayerTransform(GameObject root)
    {
        var controller = root.GetComponentInChildren<PlayerController>(true);
        return controller != null ? controller.transform : root.transform;
    }

    private static void DestroyChildIfExists(Transform parent, string childName)
    {
        var child = parent.Find(childName);
        if (child != null)
            Object.DestroyImmediate(child.gameObject);
    }

    private static void ClearBrokenReferences(GameObject root)
    {
        var controller = root.GetComponentInChildren<PlayerController>(true);
        if (controller != null)
        {
            var so = new SerializedObject(controller);
            so.FindProperty("deathHandling").objectReferenceValue = null;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        var stats = root.GetComponentInChildren<PlayerStats>(true);
        if (stats != null)
        {
            var so = new SerializedObject(stats);
            so.FindProperty("coinText").objectReferenceValue = null;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void WireCanvasButtonsToUIManager(GameObject playerCanvas, UIManager uiManager)
    {
        WireButton(FindButton(playerCanvas, "ResumeBtn"), uiManager.ResumeGame);
        WireButton(FindButton(playerCanvas, "OptionsBtn"), uiManager.OpenOptions);
        WireButton(FindButton(playerCanvas, "BackBtn"), uiManager.CloseOptions);
        WireButton(FindButton(playerCanvas, "MainMenuBtn"), uiManager.LoadMainMenu);
    }

    private static void WireDeathCanvasButtonsToUIButtons(GameObject deathCanvas, UIButtons uiButtons)
    {
        WireButton(FindButton(deathCanvas, "RetryButton"), uiButtons.retry);
        WireButton(FindButton(deathCanvas, "MainMenuButton"), uiButtons.MainMenu);
        WireButton(FindButton(deathCanvas, "QuitButton"), uiButtons.QuitApplication);
    }

    private static Button FindButton(GameObject root, string namePart)
    {
        foreach (var button in root.GetComponentsInChildren<Button>(true))
        {
            if (button.gameObject.name.Replace(" ", string.Empty).Contains(namePart))
                return button;
        }

        return null;
    }

    private static void WireButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null || action == null)
            return;

        for (int i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEditor.Events.UnityEventTools.RemovePersistentListener(button.onClick, i);

        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(button.onClick, action);
    }
}
#endif
