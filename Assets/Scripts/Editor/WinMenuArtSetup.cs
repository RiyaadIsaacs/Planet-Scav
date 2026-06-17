#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class WinMenuArtSetup
{
    private const string CoverPath = "Assets/UI/WinMenu/planet-scav-win-menu-cover.png";
    private const string ButtonPath = "Assets/UI/WinMenu/planet-scav-win-button-main-menu.png";
    private const string PrefabPath = "Assets/Prefab/UI Canvas/WinCanvas.prefab";

    [MenuItem("Planet Scav/Wire Win Menu Art")]
    public static void WireWinMenuArt()
    {
        var cover = AssetDatabase.LoadAssetAtPath<Texture2D>(CoverPath);
        var buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonPath);
        if (cover == null || buttonSprite == null)
        {
            Debug.LogError("WinMenuArtSetup: missing art in Assets/UI/WinMenu.");
            return;
        }

        var prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
        if (prefabRoot == null)
        {
            Debug.LogError($"WinMenuArtSetup: missing prefab at {PrefabPath}.");
            return;
        }

        var coverImage = prefabRoot.transform.Find("WinMenuCover")?.GetComponent<RawImage>();
        if (coverImage != null)
            coverImage.texture = cover;

        var buttonImage = prefabRoot.transform.Find("MainMenuButton")?.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.sprite = buttonSprite;
            buttonImage.preserveAspect = false;
        }

        var label = prefabRoot.transform.Find("MainMenuButton/Text (TMP)")?.gameObject;
        if (label != null)
            label.SetActive(false);

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);
        AssetDatabase.SaveAssets();

        Debug.Log("Win menu art wired on WinCanvas.prefab.");
    }
}
#endif
