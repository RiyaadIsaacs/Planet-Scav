using UnityEngine;

/// <summary>
/// Ensures the SFXManager prefab exists when any scene loads.
/// </summary>
public static class SFXManagerBootstrap
{
    private const string ResourcePath = "SFXManager";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureSFXManager()
    {
        if (SFXManager.Instance != null)
            return;

        var prefab = Resources.Load<GameObject>(ResourcePath);
        if (prefab == null)
        {
            Debug.LogWarning($"SFXManagerBootstrap: no prefab at Resources/{ResourcePath}. Run Planet Scav > Setup Sound Manager.");
            return;
        }

        Object.Instantiate(prefab);
    }
}
