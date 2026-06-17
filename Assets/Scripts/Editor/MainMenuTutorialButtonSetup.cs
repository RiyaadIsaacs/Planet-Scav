#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class MainMenuTutorialButtonSetup
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

    [MenuItem("Planet Scav/Add Tutorial Button To Main Menu")]
    public static void AddTutorialButton()
    {
        var scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

        if (GameObject.Find("TutorialButton") != null)
        {
            Debug.Log("Main menu already has a TutorialButton.");
            return;
        }

        var beginnerButton = GameObject.Find("BeginnerButton");
        if (beginnerButton == null)
        {
            Debug.LogError("MainMenuTutorialButtonSetup: BeginnerButton not found.");
            return;
        }

        var menuButtons = Object.FindFirstObjectByType<MainMenuButtons>();
        if (menuButtons == null)
        {
            Debug.LogError("MainMenuTutorialButtonSetup: MainMenuButtons not found.");
            return;
        }

        var tutorialButton = Object.Instantiate(beginnerButton, beginnerButton.transform.parent);
        tutorialButton.name = "TutorialButton";

        var tutorialRect = tutorialButton.GetComponent<RectTransform>();
        var beginnerRect = beginnerButton.GetComponent<RectTransform>();
        tutorialRect.anchoredPosition = new Vector2(
            beginnerRect.anchoredPosition.x,
            beginnerRect.anchoredPosition.y + 40f);

        var label = tutorialButton.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
            label.text = "Tutorial";

        var button = tutorialButton.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        UnityEventTools.AddPersistentListener(button.onClick, menuButtons.LoadTutorialLevel);
        EditorUtility.SetDirty(button);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Added TutorialButton to Main Menu (loads Tutorial scene).");
    }
}
#endif
