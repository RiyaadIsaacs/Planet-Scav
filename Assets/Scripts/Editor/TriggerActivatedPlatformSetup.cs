#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TriggerActivatedPlatformSetup
{
    private const string PrefabPath = "Assets/Prefab/TriggerActivatedPlatform.prefab";
    private const string AdvancedScenePath = "Assets/Scenes/Advanced.unity";

    [MenuItem("Planet Scav/Create Trigger Activated Platform Prefab")]
    public static void CreatePrefab()
    {
        var platformRoot = BuildPlatformHierarchy();
        try
        {
            PrefabUtility.SaveAsPrefabAsset(platformRoot, PrefabPath);
            Debug.Log($"Created {PrefabPath}");
        }
        finally
        {
            Object.DestroyImmediate(platformRoot);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Planet Scav/Place Trigger Platform In Advanced Scene")]
    public static void PlaceInAdvancedScene()
    {
        if (!System.IO.File.Exists(PrefabPath))
            CreatePrefab();

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError($"Missing prefab at {PrefabPath}");
            return;
        }

        var scene = EditorSceneManager.OpenScene(AdvancedScenePath, OpenSceneMode.Single);
        var spawn = GameObject.FindGameObjectWithTag("Spawn");
        var position = spawn != null
            ? spawn.transform.position + spawn.transform.forward * 8f + Vector3.up * 0.5f
            : new Vector3(10f, 30f, 0f);

        var instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
        if (instance == null)
            return;

        instance.transform.position = position;
        instance.name = "TriggerActivatedPlatform";
        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = instance;
        Debug.Log($"Placed TriggerActivatedPlatform in Advanced scene at {position}");
    }

    private static GameObject BuildPlatformHierarchy()
    {
        var platform = new GameObject("TriggerActivatedPlatform");
        platform.tag = "Platform";

        var platformCollider = platform.AddComponent<BoxCollider>();
        platformCollider.size = new Vector3(4f, 0.4f, 2f);

        var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(platform.transform, false);
        visual.transform.localScale = new Vector3(4f, 0.4f, 2f);
        Object.DestroyImmediate(visual.GetComponent<Collider>());

        var platformScript = platform.AddComponent<TriggerActivatedPlatform>();

        var startPoint = new GameObject("StartPoint");
        startPoint.transform.SetParent(platform.transform, false);
        startPoint.transform.localPosition = Vector3.zero;

        var endPoint = new GameObject("EndPoint");
        endPoint.transform.SetParent(platform.transform, false);
        endPoint.transform.localPosition = new Vector3(8f, 0f, 0f);

        var triggerGo = new GameObject("PlayerTrigger");
        triggerGo.transform.SetParent(platform.transform, false);
        triggerGo.transform.localPosition = new Vector3(-2f, 1f, 0f);
        var triggerCollider = triggerGo.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector3(3f, 2f, 3f);
        var triggerScript = triggerGo.AddComponent<PlatformPlayerTrigger>();

        var platformSo = new SerializedObject(platformScript);
        platformSo.FindProperty("startPoint").objectReferenceValue = startPoint.transform;
        platformSo.FindProperty("endPoint").objectReferenceValue = endPoint.transform;
        platformSo.ApplyModifiedPropertiesWithoutUndo();

        var triggerSo = new SerializedObject(triggerScript);
        triggerSo.FindProperty("platform").objectReferenceValue = platformScript;
        triggerSo.ApplyModifiedPropertiesWithoutUndo();

        return platform;
    }
}
#endif
