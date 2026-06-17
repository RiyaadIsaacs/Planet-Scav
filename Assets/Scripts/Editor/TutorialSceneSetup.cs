#if UNITY_EDITOR
using System.IO;
using UnityEditor.Events;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class TutorialSceneSetup
{
    private const string ScenePath = "Assets/Scenes/Tutorial.unity";
    private const string LevelRootPath = "Assets/Prefab/LevelGameplayRoot.prefab";
    private const string SpawnPath = "Assets/Prefab/Spawn.prefab";

    private static readonly (string title, string message)[] TutorialMessages =
    {
        ("Movement", "Use WASD to move around.\nHold Left Shift to sprint."),
        ("Camera", "Move the mouse to look around and explore."),
        ("Jumping", "Press Space to jump.\nHold Ctrl and press Space to charge a higher jump."),
        ("Interact", "Press E to interact with objects when you are nearby.\nTry it on the glowing terminal ahead."),
        ("Ready to Go", "Great work! Walk through the green exit ahead to begin your mission on the planet.")
    };

    [MenuItem("Planet Scav/Setup Tutorial Scene")]
    public static void SetupTutorialScene()
    {
        EnsureScenesFolder();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Tutorial";

        CreateLighting();
        CreateEventSystem();
        CreateLocalizationManager();
        CreateLevelGameplayRoot();
        CreateSpawn(new Vector3(0f, 1.5f, -5f));
        CreateTutorialGeometry();
        var tutorialUI = CreateTutorialUI();
        CreateTutorialZones(tutorialUI);
        CreatePracticeTerminal(new Vector3(0f, 1.5f, 38f));
        CreateNextLevelTrigger(new Vector3(0f, 2f, 58f));

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath, insertAtIndex: 2);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Tutorial Scene",
            "Tutorial scene created at Assets/Scenes/Tutorial.unity\n\n" +
            "It includes 5 tutorial canvases, practice interact object, and a next-level trigger to Beginner.\n\n" +
            "Wire a Main Menu button to MainMenuButtons.LoadTutorialLevel() if needed.",
            "OK");
    }

    private static void EnsureScenesFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
    }

    private static void CreateLighting()
    {
        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static void CreateEventSystem()
    {
        var eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<InputSystemUIInputModule>();
    }

    private static void CreateLocalizationManager()
    {
        var go = new GameObject("LocalizationManager");
        go.AddComponent<LocalizationManager>();
    }

    private static void CreateLevelGameplayRoot()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LevelRootPath);
        if (prefab == null)
        {
            Debug.LogError($"TutorialSceneSetup: missing prefab at {LevelRootPath}");
            return;
        }

        var root = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        root.name = "LevelGameplayRoot";

        var levelConfig = root.GetComponent<LevelConfig>();
        if (levelConfig != null)
        {
            levelConfig.dialogueSequence = null;
            levelConfig.localizationFile = "Tutorial";
            levelConfig.enablePlayerShooting = false;
        }
    }

    private static void CreateSpawn(Vector3 position)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SpawnPath);
        if (prefab == null)
        {
            Debug.LogError($"TutorialSceneSetup: missing prefab at {SpawnPath}");
            return;
        }

        var spawn = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        spawn.name = "Spawn";
        spawn.transform.position = position;
    }

    private static void CreateTutorialGeometry()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Tutorial Ground";
        ground.transform.position = new Vector3(0f, -0.5f, 25f);
        ground.transform.localScale = new Vector3(14f, 1f, 80f);

        var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp.name = "Jump Practice Ramp";
        ramp.transform.position = new Vector3(4f, 0.25f, 22f);
        ramp.transform.localScale = new Vector3(3f, 0.5f, 6f);
        ramp.transform.rotation = Quaternion.Euler(0f, 0f, -12f);

        var exitPad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        exitPad.name = "Exit Platform";
        exitPad.transform.position = new Vector3(0f, 0f, 58f);
        exitPad.transform.localScale = new Vector3(8f, 0.2f, 8f);

        var exitRenderer = exitPad.GetComponent<Renderer>();
        if (exitRenderer != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.2f, 0.85f, 0.35f);
            exitRenderer.sharedMaterial = mat;
            AssetDatabase.CreateAsset(mat, "Assets/Materials/TutorialExit.mat");
        }

        CreateZoneMarker("Zone Marker 2", new Vector3(0f, 0.1f, 10f), new Color(0.3f, 0.6f, 1f, 0.35f));
        CreateZoneMarker("Zone Marker 3", new Vector3(0f, 0.1f, 22f), new Color(0.3f, 0.6f, 1f, 0.35f));
        CreateZoneMarker("Zone Marker 4", new Vector3(0f, 0.1f, 35f), new Color(0.3f, 0.6f, 1f, 0.35f));
        CreateZoneMarker("Zone Marker 5", new Vector3(0f, 0.1f, 50f), new Color(0.3f, 0.6f, 1f, 0.35f));
    }

    private static void CreateZoneMarker(string name, Vector3 position, Color color)
    {
        var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        marker.name = name;
        marker.transform.position = position;
        marker.transform.localScale = new Vector3(10f, 0.05f, 4f);
        Object.DestroyImmediate(marker.GetComponent<Collider>());

        var renderer = marker.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            renderer.sharedMaterial = mat;
        }
    }

    private static TutorialUIManager CreateTutorialUI()
    {
        var root = new GameObject("Tutorial UI");
        var manager = root.AddComponent<TutorialUIManager>();
        var font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

        var steps = new TutorialUIManager.TutorialStep[TutorialMessages.Length];
        for (int i = 0; i < TutorialMessages.Length; i++)
        {
            steps[i] = CreateTutorialCanvas(root.transform, i, TutorialMessages[i].title, TutorialMessages[i].message, font, manager);
        }

        var managerSo = new SerializedObject(manager);
        managerSo.FindProperty("steps").arraySize = steps.Length;
        for (int i = 0; i < steps.Length; i++)
        {
            var element = managerSo.FindProperty("steps").GetArrayElementAtIndex(i);
            element.FindPropertyRelative("canvas").objectReferenceValue = steps[i].canvas;
            element.FindPropertyRelative("titleText").objectReferenceValue = steps[i].titleText;
            element.FindPropertyRelative("messageText").objectReferenceValue = steps[i].messageText;
            element.FindPropertyRelative("title").stringValue = steps[i].title;
            element.FindPropertyRelative("message").stringValue = steps[i].message;
        }

        managerSo.FindProperty("showFirstStepOnStart").boolValue = true;
        managerSo.FindProperty("firstStepDelay").floatValue = 1f;
        managerSo.ApplyModifiedPropertiesWithoutUndo();

        return manager;
    }

    private static TutorialUIManager.TutorialStep CreateTutorialCanvas(
        Transform parent,
        int index,
        string title,
        string message,
        TMP_FontAsset font,
        TutorialUIManager manager)
    {
        var canvasGo = new GameObject($"TutorialCanvas_{index + 1}");
        canvasGo.transform.SetParent(parent, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 120 + index;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasGo.AddComponent<GraphicRaycaster>();

        var panel = CreateUiRect("Panel", canvasGo.transform, new Vector2(0.15f, 0.2f), new Vector2(0.85f, 0.8f));
        var panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.08f, 0.12f, 0.92f);

        var titleText = CreateTmpText("Title", panel, font, 42, FontStyles.Bold, new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.92f), title);
        var messageText = CreateTmpText("Message", panel, font, 30, FontStyles.Normal, new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.68f), message);
        messageText.alignment = TextAlignmentOptions.TopLeft;

        var buttonRect = CreateUiRect("GotItButton", panel, new Vector2(0.35f, 0.08f), new Vector2(0.65f, 0.18f));
        var buttonImage = buttonRect.gameObject.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.55f, 0.95f, 1f);
        var button = buttonRect.gameObject.AddComponent<Button>();

        var buttonLabel = CreateTmpText("Label", buttonRect, font, 28, FontStyles.Bold, Vector2.zero, Vector2.one, "Got it");
        buttonLabel.alignment = TextAlignmentOptions.Center;

        button.onClick.RemoveAllListeners();
        UnityEventTools.AddPersistentListener(button.onClick, manager.HideCurrentStep);
        EditorUtility.SetDirty(button);
        canvasGo.SetActive(false);

        return new TutorialUIManager.TutorialStep
        {
            canvas = canvas,
            titleText = titleText,
            messageText = messageText,
            title = title,
            message = message
        };
    }

    private static RectTransform CreateUiRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    private static TMP_Text CreateTmpText(
        string name,
        Transform parent,
        TMP_FontAsset font,
        float fontSize,
        FontStyles style,
        Vector2 anchorMin,
        Vector2 anchorMax,
        string text)
    {
        var rect = CreateUiRect(name, parent, anchorMin, anchorMax);
        var tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = Color.white;
        tmp.text = text;
        tmp.enableWordWrapping = true;
        return tmp;
    }

    private static void CreateTutorialZones(TutorialUIManager tutorialUI)
    {
        CreateTutorialZone("Tutorial Zone 2 - Camera", new Vector3(0f, 2f, 10f), new Vector3(12f, 6f, 6f), tutorialUI, 1);
        CreateTutorialZone("Tutorial Zone 3 - Jump", new Vector3(0f, 2f, 22f), new Vector3(12f, 6f, 6f), tutorialUI, 2);
        CreateTutorialZone("Tutorial Zone 4 - Interact", new Vector3(0f, 2f, 35f), new Vector3(12f, 6f, 6f), tutorialUI, 3);
        CreateTutorialZone("Tutorial Zone 5 - Exit", new Vector3(0f, 2f, 50f), new Vector3(12f, 6f, 8f), tutorialUI, 4);
    }

    private static void CreateTutorialZone(string name, Vector3 position, Vector3 size, TutorialUIManager tutorialUI, int stepIndex)
    {
        var zone = new GameObject(name);
        zone.transform.position = position;

        var collider = zone.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = size;

        var trigger = zone.AddComponent<TutorialStepTrigger>();
        var so = new SerializedObject(trigger);
        so.FindProperty("tutorialUI").objectReferenceValue = tutorialUI;
        so.FindProperty("stepIndex").intValue = stepIndex;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreatePracticeTerminal(Vector3 position)
    {
        var terminal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        terminal.name = "Practice Terminal";
        terminal.transform.position = position;
        terminal.transform.localScale = new Vector3(1.2f, 1.5f, 1.2f);

        var renderer = terminal.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.2f, 0.85f, 1f);
            renderer.sharedMaterial = mat;
            AssetDatabase.CreateAsset(mat, "Assets/Materials/TutorialTerminal.mat");
        }

        var collider = terminal.GetComponent<Collider>();
        collider.isTrigger = true;

        var glow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        glow.name = "Activated Visual";
        glow.transform.SetParent(terminal.transform, false);
        glow.transform.localPosition = new Vector3(0f, 1.2f, 0f);
        glow.transform.localScale = Vector3.one * 0.6f;
        Object.DestroyImmediate(glow.GetComponent<Collider>());
        glow.SetActive(false);

        var glowRenderer = glow.GetComponent<Renderer>();
        if (glowRenderer != null)
        {
            var glowMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            glowMat.color = new Color(0.4f, 1f, 0.5f);
            glowRenderer.sharedMaterial = glowMat;
        }

        var practice = terminal.AddComponent<TutorialPracticeInteractable>();
        var practiceSo = new SerializedObject(practice);
        practiceSo.FindProperty("activatedVisual").objectReferenceValue = glow;
        practiceSo.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateNextLevelTrigger(Vector3 position)
    {
        var triggerGo = new GameObject("Next Level Trigger");
        triggerGo.transform.position = position;

        var collider = triggerGo.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(6f, 4f, 6f);

        var trigger = triggerGo.AddComponent<NextLevelTrigger>();
        var so = new SerializedObject(trigger);
        so.FindProperty("nextSceneName").stringValue = "Beginner";
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddSceneToBuildSettings(string scenePath, int insertAtIndex)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var existing in scenes)
        {
            if (existing.path == scenePath)
                return;
        }

        if (!File.Exists(scenePath))
            return;

        scenes.Insert(Mathf.Clamp(insertAtIndex, 0, scenes.Count), new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
#endif
