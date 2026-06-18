#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuBriefSetup
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string MenuManagerName = "MenuManager";

    [MenuItem("Planet Scav/Setup Main Menu (POE Brief)")]
    public static void SetupMainMenu()
    {
        var scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("MainMenuBriefSetup: no Canvas found in Main Menu scene.");
            return;
        }

        var menuManager = Object.FindFirstObjectByType<MainMenuButtons>();
        if (menuManager == null)
        {
            var managerGo = GameObject.Find(MenuManagerName) ?? new GameObject(MenuManagerName);
            menuManager = managerGo.GetComponent<MainMenuButtons>() ?? managerGo.AddComponent<MainMenuButtons>();
        }

        DestroyIfExists(canvas.transform, "SceneButton");
        DestroyIfExists(canvas.transform, "PlayButton");

        var beginner = EnsureButton(canvas.transform, "BeginnerButton", "Beginner Level", new Vector2(0f, 55f));
        var advanced = EnsureButton(canvas.transform, "AdvancedButton", "Moving Platform Level", new Vector2(0f, 15f));
        var final = EnsureButton(canvas.transform, "FinalButton", "Final Level", new Vector2(0f, -25f));
        var quit = EnsureButton(canvas.transform, "QuitButton", "Quit", new Vector2(0f, -65f));

        WireButton(beginner, menuManager.LoadBeginnerLevel);
        WireButton(advanced, menuManager.LoadAdvancedLevel);
        WireButton(final, menuManager.LoadFinalLevel);
        WireButton(quit, menuManager.QuitGame);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Main Menu updated: Beginner, Moving Platform, Final, and Quit buttons are wired.");
    }

    private static void DestroyIfExists(Transform parent, string objectName)
    {
        var existing = parent.Find(objectName);
        if (existing != null)
            Object.DestroyImmediate(existing.gameObject);
    }

    private static Button EnsureButton(Transform parent, string objectName, string label, Vector2 anchoredPosition)
    {
        var existing = parent.Find(objectName);
        GameObject buttonGo;

        if (existing != null)
        {
            buttonGo = existing.gameObject;
        }
        else
        {
            buttonGo = DefaultControls.CreateButton(GetDefaultResources());
            buttonGo.name = objectName;
            buttonGo.transform.SetParent(parent, false);
        }

        var rect = buttonGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(260f, 36f);

        var labelText = buttonGo.GetComponentInChildren<TMP_Text>();
        if (labelText != null)
            labelText.text = label;

        return buttonGo.GetComponent<Button>();
    }

    private static void WireButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null || action == null)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private static DefaultControls.Resources GetDefaultResources()
    {
        return new DefaultControls.Resources
        {
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
            inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
            checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd"),
            dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd"),
            mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd")
        };
    }
}
#endif
