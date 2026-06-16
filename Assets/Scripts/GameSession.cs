using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Spawns and owns the persistent player across gameplay scenes.
/// Menu scenes never contain a player; gameplay scenes never place one in the scene file.
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameSession : MonoBehaviour
{
    private static GameSession instance;

    [SerializeField] private GameObject playerPrefabOverride;

    private GameObject playerInstance;
    private LevelReferenceBinder levelBinder;
    private GameSessionConfig config;

    public static GameSession Instance => instance;
    public GameObject Player => playerInstance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindAnyObjectByType<GameSession>() != null)
            return;

        var sessionObject = new GameObject("GameSession");
        sessionObject.AddComponent<GameSession>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        config = Resources.Load<GameSessionConfig>("GameSessionConfig");
        playerPrefabOverride = ResolvePlayerPrefab();

        if (playerPrefabOverride == null)
            Debug.LogError("GameSession: no player prefab found. Run Planet Scav > Ensure GameSession Config in the editor.");
    }

    private void Start()
    {
        StartCoroutine(ProcessSceneWhenReady(SceneManager.GetActiveScene()));
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (instance == this)
            instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ProcessSceneWhenReady(scene));
    }

    private IEnumerator ProcessSceneWhenReady(Scene scene)
    {
        yield return null;

        if (!scene.IsValid() || !scene.isLoaded)
            yield break;

        ProcessScene(scene);
    }

    private void ProcessScene(Scene scene)
    {
        if (IsMenuScene(scene.name))
        {
            DestroyPlayer();
            Time.timeScale = 1f;
            AudioListener.pause = false;
            return;
        }

        RemoveScenePlacedPlayers();
        EnsurePlayer();
        PositionPlayerAtSpawn();
        levelBinder?.BindSceneReferences();
    }

    private GameObject ResolvePlayerPrefab()
    {
#if UNITY_EDITOR
        var editorPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (editorPrefab != null)
            return editorPrefab;
#endif

        if (config != null && config.playerPrefab is GameObject configPrefab)
            return configPrefab;

        return null;
    }

    private const string PlayerPrefabPath = "Assets/Prefab/Player.prefab";

    private bool IsMenuScene(string sceneName)
    {
        if (config == null || config.menuSceneNames == null || config.menuSceneNames.Length == 0)
            return sceneName == "MainMenu" || sceneName == "Scene Select";

        foreach (var menuScene in config.menuSceneNames)
        {
            if (menuScene == sceneName)
                return true;
        }

        return false;
    }

    private void RemoveScenePlacedPlayers()
    {
        var controllers = FindObjectsByType<PlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var controller in controllers)
        {
            if (playerInstance != null && controller.gameObject == playerInstance)
                continue;

            Destroy(controller.gameObject);
        }
    }

    private void EnsurePlayer()
    {
        if (playerInstance != null)
            return;

        var prefab = playerPrefabOverride;
        if (prefab == null)
            return;

        playerInstance = (GameObject)Instantiate(prefab);
        playerInstance.name = "Player";
        playerInstance.SetActive(true);

        levelBinder = playerInstance.GetComponent<LevelReferenceBinder>();
        if (levelBinder == null)
            levelBinder = playerInstance.AddComponent<LevelReferenceBinder>();
    }

    private void PositionPlayerAtSpawn()
    {
        if (playerInstance == null)
            return;

        var spawn = GameObject.FindGameObjectWithTag("Spawn");
        if (spawn == null)
        {
            Debug.LogWarning($"GameSession: no object tagged 'Spawn' in scene '{SceneManager.GetActiveScene().name}'.");
            return;
        }

        playerInstance.transform.SetPositionAndRotation(spawn.transform.position, spawn.transform.rotation);
    }

    private void DestroyPlayer()
    {
        if (playerInstance == null)
            return;

        Destroy(playerInstance);
        playerInstance = null;
        levelBinder = null;
    }
}
