#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class MainMenuArtSetup
{
    private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string ArtFolder = "Assets/UI/MainMenu";

    private static readonly (string buttonName, string spriteFile)[] ButtonArt =
    {
        ("TutorialButton", "planet-scav-button-tutorial.png"),
        ("BeginnerButton", "planet-scav-button-beginner.png"),
        ("AdvancedButton", "planet-scav-button-advanced.png"),
        ("FinalButton", "planet-scav-button-final.png"),
        ("QuitButton", "planet-scav-button-quit.png"),
    };

    [MenuItem("Planet Scav/Wire Main Menu Art")]
    public static void WireMainMenuArt()
    {
        ConfigureTextures();

        var scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
        var canvas = GameObject.Find("Canvas")?.GetComponent<RectTransform>();
        if (canvas == null)
        {
            Debug.LogError("MainMenuArtSetup: Canvas not found.");
            return;
        }

        EnsureCoverImage(canvas);
        StyleLevelButtons();
        HideLegacyElements();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("Main menu art wired (cover + button sprites).");
    }

    private static void ConfigureTextures()
    {
        var coverPath = $"{ArtFolder}/planet-scav-main-menu-cover.png";
        var coverImporter = AssetImporter.GetAtPath(coverPath) as TextureImporter;
        if (coverImporter != null)
        {
            coverImporter.textureType = TextureImporterType.Default;
            coverImporter.spriteImportMode = SpriteImportMode.None;
            coverImporter.alphaIsTransparency = true;
            coverImporter.mipmapEnabled = false;
            coverImporter.wrapMode = TextureWrapMode.Clamp;
            coverImporter.SaveAndReimport();
        }

        foreach (var (_, spriteFile) in ButtonArt)
        {
            var path = $"{ArtFolder}/{spriteFile}";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }
    }

    private static void EnsureCoverImage(RectTransform canvas)
    {
        var coverGo = GameObject.Find("MenuCoverImage");
        if (coverGo == null)
        {
            coverGo = new GameObject("MenuCoverImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            coverGo.transform.SetParent(canvas, false);
            coverGo.transform.SetAsFirstSibling();
        }

        var coverRect = coverGo.GetComponent<RectTransform>();
        StretchFullScreen(coverRect);

        var rawImage = coverGo.GetComponent<RawImage>();
        var coverTexture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{ArtFolder}/planet-scav-main-menu-cover.png");
        rawImage.texture = coverTexture;
        rawImage.raycastTarget = false;
        rawImage.color = Color.white;
        EditorUtility.SetDirty(rawImage);
    }

    private static void StyleLevelButtons()
    {
        const float buttonWidth = 170f;
        const float buttonGap = 6f;

        var referenceSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/{ButtonArt[0].spriteFile}");
        if (referenceSprite == null)
            return;

        var aspect = referenceSprite.rect.height / referenceSprite.rect.width;
        var buttonSize = new Vector2(buttonWidth, buttonWidth * aspect);
        var step = buttonSize.y + buttonGap;
        var y = step * (ButtonArt.Length - 1) * 0.5f;

        foreach (var (buttonName, spriteFile) in ButtonArt)
        {
            var buttonGo = GameObject.Find(buttonName);
            if (buttonGo == null)
            {
                Debug.LogWarning($"MainMenuArtSetup: {buttonName} not found.");
                y -= step;
                continue;
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/{spriteFile}");
            if (sprite == null)
            {
                Debug.LogWarning($"MainMenuArtSetup: sprite not found for {buttonName}.");
                y -= step;
                continue;
            }

            var rect = buttonGo.GetComponent<RectTransform>();
            rect.sizeDelta = buttonSize;
            rect.anchoredPosition = new Vector2(0f, y);
            y -= step;

            var image = buttonGo.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            EditorUtility.SetDirty(image);

            var label = buttonGo.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.gameObject.SetActive(false);
        }
    }

    private static void HideLegacyElements()
    {
        var backdrop = GameObject.Find("BackDropPlane");
        if (backdrop != null)
            backdrop.SetActive(false);

        var title = canvasTitleText();
        if (title != null)
            title.gameObject.SetActive(false);
    }

    private static TMP_Text canvasTitleText()
    {
        var canvas = GameObject.Find("Canvas");
        if (canvas == null)
            return null;

        foreach (var text in canvas.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.gameObject.name == "Text (TMP)" && text.transform.parent == canvas.transform)
                return text;
        }

        return null;
    }

    private static void StretchFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }
}
#endif
