#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildSafetyTips
{
    [MenuItem("Planet Scav/Build Tips (Read Me)")]
    public static void ShowTips()
    {
        EditorUtility.DisplayDialog(
            "Safer Builds for Planet Scav",
            "If a build freezes or crashes your PC:\n\n" +
            "1. Close other heavy apps (browsers, games, extra Unity windows).\n" +
            "2. File > Build Settings > uncheck Development Build.\n" +
            "3. In Unity Hub, use a stable LTS editor version for this project.\n" +
            "4. Update your GPU driver (builds stress graphics + compilation).\n" +
            "5. Ensure Windows page file is system-managed (Settings > System > About > Advanced system settings > Performance > Advanced > Virtual memory).\n\n" +
            "This project’s menu UI art is now imported at 1024px max to reduce memory use during builds.\n\n" +
            "A full PC reboot usually means RAM/GPU/driver pressure, not a normal Unity script error.",
            "OK");
    }

    [MenuItem("Planet Scav/Optimize UI Art Import Sizes")]
    public static void OptimizeUiArtImports()
    {
        const string artRoot = "Assets/UI";
        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { artRoot });
        var changed = 0;

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                continue;

            if (importer.maxTextureSize <= 1024)
                continue;

            importer.maxTextureSize = 1024;
            importer.SaveAndReimport();
            changed++;
        }

        Debug.Log($"Planet Scav: optimized import size for {changed} UI texture(s).");
    }
}
#endif
