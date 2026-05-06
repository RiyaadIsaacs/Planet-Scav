using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroy : MonoBehaviour
{
    private static DontDestroy instance;
    public Transform spawnPoint;

    private void Awake()
    {
        // Prevent duplicates if player exists in next scene too
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject spawn = GameObject.FindGameObjectWithTag("Spawn");
        if (spawn == null)
        {
            Debug.LogWarning($"No object tagged 'Spawn' in scene: {scene.name}");
            return;
        }

        spawnPoint = spawn.transform;
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
    }
}