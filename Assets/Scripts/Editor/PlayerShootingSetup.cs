#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[InitializeOnLoad]
public static class PlayerShootingSetupAutoRun
{
    static PlayerShootingSetupAutoRun()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (PlayerShootingSetup.NeedsFix())
            {
                Debug.Log("Planet Scav: repairing Player animator (generic rig + upper-body mask).");
                PlayerShootingSetup.FixSetup();
            }
        };
    }
}

public static class PlayerShootingSetup
{
    private const string PlayerPrefabPath = "Assets/Prefab/Player.prefab";
    private const string VanguardModelPath = "Assets/Imports/Player Model/Vanguard By T. Choonyung.fbx";
    private const string MaskPath = "Assets/Animation/UpperBody.mask";
    private const string ControllerPath = "Assets/Animation/Player Animator Controller.controller";
    private const string UpperBodyLayerName = "UpperBody";

    private static readonly string[] UpperBodyBoneTokens =
    {
        "Spine", "Neck", "Head", "Shoulder", "Arm", "ForeArm", "Hand",
        "Thumb", "Index", "Middle", "Ring", "Pinky"
    };

    private static readonly string[] LowerBodyBoneTokens =
    {
        "Hips", "UpLeg", "Leg", "Foot", "ToeBase"
    };

    [MenuItem("Planet Scav/Fix Player Shooting Setup")]
    public static void FixSetupMenu()
    {
        FixSetup();
        EditorUtility.DisplayDialog(
            "Player Shooting Setup",
            "Player animator restored for Generic clips.\n\n" +
            "Movement clips and upper-body shooting should work again. " +
            "Test on Final Level: walk/run while shooting.",
            "OK");
    }

    public static bool NeedsFix()
    {
        if (!System.IO.File.Exists(PlayerPrefabPath))
            return false;

        if (GetPlayerPrefabAvatar() != null)
            return true;

        if (IsVanguardHumanoid())
            return true;

        if (AssetDatabase.LoadAssetAtPath<AvatarMask>(MaskPath) == null)
            return true;

        return ControllerUpperBodyMaskMissing();
    }

    public static void FixSetup()
    {
        RevertVanguardToGeneric();

        var root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        try
        {
            ClearAnimatorAvatar(root);
            var mask = CreateOrReplaceUpperBodyMask(root.transform);
            WireAnimatorController(mask);
            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("PlayerShootingSetup: generic animator + transform upper-body mask restored.");
    }

    private static Avatar GetPlayerPrefabAvatar()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (prefab == null)
            return null;

        var animator = prefab.GetComponentInChildren<Animator>(true);
        return animator != null ? animator.avatar : null;
    }

    private static bool IsVanguardHumanoid()
    {
        var importer = AssetImporter.GetAtPath(VanguardModelPath) as ModelImporter;
        return importer != null && importer.animationType == ModelImporterAnimationType.Human;
    }

    private static bool ControllerUpperBodyMaskMissing()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null || controller.layers.Length < 2)
            return true;

        for (int i = 0; i < controller.layers.Length; i++)
        {
            if (controller.layers[i].name != UpperBodyLayerName)
                continue;

            return controller.layers[i].avatarMask == null;
        }

        return true;
    }

    private static void RevertVanguardToGeneric()
    {
        var importer = AssetImporter.GetAtPath(VanguardModelPath) as ModelImporter;
        if (importer == null)
            return;

        if (importer.animationType == ModelImporterAnimationType.Generic &&
            importer.avatarSetup == ModelImporterAvatarSetup.NoAvatar)
            return;

        importer.animationType = ModelImporterAnimationType.Generic;
        importer.avatarSetup = ModelImporterAvatarSetup.NoAvatar;
        importer.SaveAndReimport();
    }

    private static void ClearAnimatorAvatar(GameObject root)
    {
        var animator = root.GetComponentInChildren<Animator>(true);
        if (animator == null)
        {
            Debug.LogError("PlayerShootingSetup: Player prefab has no Animator.");
            return;
        }

        animator.avatar = null;
        EditorUtility.SetDirty(animator);
    }

    private static AvatarMask CreateOrReplaceUpperBodyMask(Transform skeletonRoot)
    {
        var existing = AssetDatabase.LoadAssetAtPath<AvatarMask>(MaskPath);
        if (existing != null)
            AssetDatabase.DeleteAsset(MaskPath);

        var mask = new AvatarMask { name = "UpperBody" };
        ConfigureGenericUpperBodyMask(mask, skeletonRoot);

        AssetDatabase.CreateAsset(mask, MaskPath);
        return AssetDatabase.LoadAssetAtPath<AvatarMask>(MaskPath);
    }

    private static void ConfigureGenericUpperBodyMask(AvatarMask mask, Transform skeletonRoot)
    {
        var transforms = new List<Transform>();
        CollectUpperBodyTransforms(skeletonRoot, transforms);

        if (transforms.Count == 0)
        {
            Debug.LogWarning("PlayerShootingSetup: no upper-body bones found; using Mixamo fallback bone names.");
            foreach (var boneName in GetMixamoFallbackBoneNames())
            {
                var bone = FindBoneByName(skeletonRoot, boneName);
                if (bone != null)
                    transforms.Add(bone);
            }
        }

        foreach (var bone in transforms.Distinct())
            mask.AddTransformPath(bone, false);
    }

    private static void CollectUpperBodyTransforms(Transform node, List<Transform> transforms)
    {
        if (IsUpperBodyBone(node.name))
            transforms.Add(node);

        for (int i = 0; i < node.childCount; i++)
            CollectUpperBodyTransforms(node.GetChild(i), transforms);
    }

    private static Transform FindBoneByName(Transform root, string boneName)
    {
        foreach (var transform in root.GetComponentsInChildren<Transform>(true))
        {
            if (transform.name == boneName)
                return transform;
        }

        return null;
    }

    private static bool IsUpperBodyBone(string boneName)
    {
        if (string.IsNullOrEmpty(boneName))
            return false;

        foreach (var token in LowerBodyBoneTokens)
        {
            if (boneName.Contains(token))
                return false;
        }

        foreach (var token in UpperBodyBoneTokens)
        {
            if (boneName.Contains(token))
                return true;
        }

        return false;
    }

    private static IEnumerable<string> GetMixamoFallbackBoneNames()
    {
        yield return "mixamorig:Spine";
        yield return "mixamorig:Spine1";
        yield return "mixamorig:Spine2";
        yield return "mixamorig:Neck";
        yield return "mixamorig:Head";
        yield return "mixamorig:LeftShoulder";
        yield return "mixamorig:LeftArm";
        yield return "mixamorig:LeftForeArm";
        yield return "mixamorig:LeftHand";
        yield return "mixamorig:RightShoulder";
        yield return "mixamorig:RightArm";
        yield return "mixamorig:RightForeArm";
        yield return "mixamorig:RightHand";
    }

    private static void WireAnimatorController(AvatarMask mask)
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null)
        {
            Debug.LogError($"PlayerShootingSetup: missing animator controller at {ControllerPath}");
            return;
        }

        var layers = controller.layers;
        var updated = false;

        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].name != UpperBodyLayerName)
                continue;

            layers[i].avatarMask = mask;
            layers[i].defaultWeight = 0f;
            layers[i].blendingMode = AnimatorLayerBlendingMode.Override;
            updated = true;
            break;
        }

        if (!updated)
        {
            Debug.LogWarning("PlayerShootingSetup: UpperBody layer not found on animator controller.");
            return;
        }

        if (layers.Length > 0)
            layers[0].defaultWeight = 1f;

        controller.layers = layers;
        EditorUtility.SetDirty(controller);
    }
}
#endif
