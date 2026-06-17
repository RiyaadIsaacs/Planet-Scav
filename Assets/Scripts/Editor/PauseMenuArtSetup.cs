#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class PauseMenuArtSetup
{
    private const string PlayerCanvasPath = "Assets/Prefab/UI Canvas/PlayerCanvas.prefab";
    private const string ArtFolder = "Assets/UI/PauseMenu";
    private const string CoverFile = "planet-scav-pause-menu-cover.png";

    private static readonly (string buttonName, string spriteFile)[] PauseButtons =
    {
        ("ResumeBtn", "planet-scav-pause-button-resume.png"),
        ("OptionsBtn", "planet-scav-pause-button-options.png"),
        ("MainMenuBtn ", "planet-scav-pause-button-main-menu.png"),
    };

    private static readonly (string buttonName, string spriteFile)[] OptionsButtons =
    {
        ("BackBtn ", "planet-scav-pause-button-back.png"),
    };

    [MenuItem("Planet Scav/Wire Pause Menu Art")]
    public static void WirePauseMenuArt()
    {
        ConfigureTextures();
        AssetDatabase.Refresh();

        var root = PrefabUtility.LoadPrefabContents(PlayerCanvasPath);
        try
        {
            var pauseMenu = root.transform.Find("PauseMenu");
            var optionsMenu = root.transform.Find("OptionsMenu");

            if (pauseMenu != null)
                WireMenu(pauseMenu, PauseButtons, "PauseTxt", "PauseMenuCover");
            else
                Debug.LogWarning("PauseMenuArtSetup: PauseMenu not found.");

            if (optionsMenu != null)
                WireMenu(optionsMenu, OptionsButtons, "OptionsTxt", "OptionsMenuCover");
            else
                Debug.LogWarning("PauseMenuArtSetup: OptionsMenu not found.");

            if (pauseMenu != null)
                pauseMenu.gameObject.SetActive(false);
            if (optionsMenu != null)
                optionsMenu.gameObject.SetActive(false);

            PrefabUtility.SaveAsPrefabAsset(root, PlayerCanvasPath);
            AssetDatabase.SaveAssets();
            Debug.Log("Pause menu art wired in PlayerCanvas.prefab.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void ConfigureTextures()
    {
        var coverPath = $"{ArtFolder}/{CoverFile}";
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

        foreach (var (_, spriteFile) in PauseButtons)
            ConfigureButtonSprite($"{ArtFolder}/{spriteFile}");
        foreach (var (_, spriteFile) in OptionsButtons)
            ConfigureButtonSprite($"{ArtFolder}/{spriteFile}");
    }

    private static void ConfigureButtonSprite(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
    }

    private static void WireMenu(Transform menuRoot, (string buttonName, string spriteFile)[] buttons, string titleName, string coverObjectName)
    {
        EnsureCoverImage(menuRoot, coverObjectName);
        HideRootOverlay(menuRoot);
        HideTitle(menuRoot, titleName);
        StyleButtons(menuRoot, buttons);
    }

    private static void EnsureCoverImage(Transform menuRoot, string coverObjectName)
    {
        var coverTransform = menuRoot.Find(coverObjectName);
        GameObject coverGo;
        if (coverTransform == null)
        {
            coverGo = new GameObject(coverObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            coverGo.transform.SetParent(menuRoot, false);
            coverGo.transform.SetAsFirstSibling();
        }
        else
        {
            coverGo = coverTransform.gameObject;
            coverGo.transform.SetAsFirstSibling();
        }

        var coverRect = coverGo.GetComponent<RectTransform>();
        StretchFullScreen(coverRect);

        var rawImage = coverGo.GetComponent<RawImage>();
        rawImage.texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{ArtFolder}/{CoverFile}");
        rawImage.raycastTarget = false;
        rawImage.color = Color.white;
        EditorUtility.SetDirty(rawImage);
    }

    private static void HideRootOverlay(Transform menuRoot)
    {
        var overlay = menuRoot.GetComponent<Image>();
        if (overlay == null)
            return;

        overlay.sprite = null;
        overlay.color = new Color(0f, 0f, 0f, 0f);
        EditorUtility.SetDirty(overlay);
    }

    private static void HideTitle(Transform menuRoot, string titleName)
    {
        var title = menuRoot.Find(titleName);
        if (title != null)
            title.gameObject.SetActive(false);
    }

    private static void StyleButtons(Transform menuRoot, (string buttonName, string spriteFile)[] buttons)
    {
        const float buttonWidth = 340f;
        const float buttonGap = 10f;

        var referenceSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/{buttons[0].spriteFile}");
        if (referenceSprite == null)
            return;

        var aspect = referenceSprite.rect.height / referenceSprite.rect.width;
        var buttonSize = new Vector2(buttonWidth, buttonWidth * aspect);
        var step = buttonSize.y + buttonGap;
        var y = step * (buttons.Length - 1) * 0.5f;

        foreach (var (buttonName, spriteFile) in buttons)
        {
            var buttonTransform = menuRoot.Find(buttonName);
            if (buttonTransform == null)
            {
                Debug.LogWarning($"PauseMenuArtSetup: {buttonName} not found under {menuRoot.name}.");
                y -= step;
                continue;
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtFolder}/{spriteFile}");
            if (sprite == null)
            {
                Debug.LogWarning($"PauseMenuArtSetup: sprite not found at {ArtFolder}/{spriteFile}.");
                y -= step;
                continue;
            }

            var rect = buttonTransform.GetComponent<RectTransform>();
            rect.localScale = Vector3.one;
            rect.sizeDelta = buttonSize;
            rect.anchoredPosition = new Vector2(0f, y - 40f);
            y -= step;

            var image = buttonTransform.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            EditorUtility.SetDirty(image);

            var label = buttonTransform.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                label.gameObject.SetActive(false);
        }
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
