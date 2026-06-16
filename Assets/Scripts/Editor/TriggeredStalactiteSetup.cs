#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class TriggeredStalactiteSetup
{
    private const string StalactitePrefabPath = "Assets/Prefab/Stalactite.prefab";
    private const string TrapPrefabPath = "Assets/Prefab/Triggered Stalactite.prefab";

    [MenuItem("Planet Scav/Create Triggered Stalactite Prefab")]
    public static void CreatePrefab()
    {
        var trapRoot = BuildTrapHierarchy();
        try
        {
            PrefabUtility.SaveAsPrefabAsset(trapRoot, TrapPrefabPath);
            Debug.Log($"Created {TrapPrefabPath}");
        }
        finally
        {
            Object.DestroyImmediate(trapRoot);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static GameObject BuildTrapHierarchy()
    {
        var stalactitePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(StalactitePrefabPath);
        if (stalactitePrefab == null)
        {
            Debug.LogError($"Missing stalactite prefab at {StalactitePrefabPath}");
            return new GameObject("TriggeredStalactite");
        }

        var trapRoot = new GameObject("TriggeredStalactite");
        trapRoot.transform.position = Vector3.zero;

        var stalactite = PrefabUtility.InstantiatePrefab(stalactitePrefab, trapRoot.transform) as GameObject;
        if (stalactite != null)
        {
            stalactite.name = "Stalactite";
            stalactite.transform.localPosition = new Vector3(0f, 0f, 0f);
        }

        var triggerGo = new GameObject("FallTrigger");
        triggerGo.transform.SetParent(trapRoot.transform, false);
        triggerGo.transform.localPosition = new Vector3(0f, -3f, 0f);

        var triggerCollider = triggerGo.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector3(4f, 2f, 4f);

        var fallTrigger = triggerGo.AddComponent<StalactiteFallTrigger>();
        var stalactiteComponent = stalactite != null
            ? stalactite.GetComponent<FallingStalactite>()
            : null;

        if (stalactiteComponent != null)
        {
            var triggerSo = new SerializedObject(fallTrigger);
            triggerSo.FindProperty("stalactites").arraySize = 1;
            triggerSo.FindProperty("stalactites").GetArrayElementAtIndex(0).objectReferenceValue =
                stalactiteComponent;
            triggerSo.FindProperty("fallDelay").floatValue = 2f;
            triggerSo.FindProperty("triggerOnce").boolValue = true;
            triggerSo.ApplyModifiedPropertiesWithoutUndo();
        }

        return trapRoot;
    }
}
#endif
