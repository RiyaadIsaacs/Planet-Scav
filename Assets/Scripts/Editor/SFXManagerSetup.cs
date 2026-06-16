#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SFXManagerSetup
{
    private const string ClickClipPath = "Assets/SFX/Click.mp3";
    private const string PrefabPath = "Assets/Resources/SFXManager.prefab";
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

    [MenuItem("Planet Scav/Setup Sound Manager")]
    public static void SetupAll()
    {
        CreateSFXManagerPrefab();
        WireButtonsInMainMenu();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog(
            "Sound Manager",
            "SFXManager prefab created at Resources/SFXManager.prefab with the Click sound.\n\n" +
            "UIButtonClickSound was added to Main Menu buttons.",
            "OK");
    }

    [MenuItem("Planet Scav/Wire UI Click Sounds In Open Scene")]
    public static void WireButtonsInOpenScene()
    {
        var count = AddClickSoundToAllButtons();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"Added UIButtonClickSound to {count} button(s) in the active scene.");
    }

    private static void CreateSFXManagerPrefab()
    {
        var clickClip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClickClipPath);
        if (clickClip == null)
        {
            Debug.LogError($"SFXManagerSetup: missing click clip at {ClickClipPath}");
            return;
        }

        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (existing != null)
            AssetDatabase.DeleteAsset(PrefabPath);

        var root = new GameObject("SFXManager");
        var audioSource = root.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        var manager = root.AddComponent<SFXManager>();
        var managerSo = new SerializedObject(manager);
        managerSo.FindProperty("persistAcrossScenes").boolValue = true;
        managerSo.FindProperty("volume").floatValue = 1f;

        var sounds = managerSo.FindProperty("sounds");
        sounds.arraySize = 1;
        sounds.GetArrayElementAtIndex(0).FindPropertyRelative("id").stringValue = SFXManager.ClickSoundId;
        sounds.GetArrayElementAtIndex(0).FindPropertyRelative("clip").objectReferenceValue = clickClip;
        managerSo.ApplyModifiedPropertiesWithoutUndo();

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        Debug.Log($"Created {PrefabPath} with sound id '{SFXManager.ClickSoundId}'.");
    }

    private static void WireButtonsInMainMenu()
    {
        if (!System.IO.File.Exists(MainMenuScenePath))
            return;

        var scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
        var count = AddClickSoundToAllButtons();
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"Main Menu: added UIButtonClickSound to {count} button(s).");
    }

    private static int AddClickSoundToAllButtons()
    {
        var count = 0;
        foreach (var button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (button.GetComponent<UIButtonClickSound>() != null)
                continue;

            var clickSound = button.gameObject.AddComponent<UIButtonClickSound>();
            var clickSo = new SerializedObject(clickSound);
            clickSo.FindProperty("soundId").stringValue = SFXManager.ClickSoundId;
            clickSo.ApplyModifiedPropertiesWithoutUndo();
            count++;
        }

        return count;
    }
}
#endif
