#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;

public static class RockMonsterSetup
{
    private const string GolemModelPath =
        "Assets/Kevin Iglesias/Characters/Humanoid Giant/Models/GiantModel01 - Golem.fbx";
    private const string GolemPrefabPath =
        "Assets/Kevin Iglesias/Characters/Humanoid Giant/Prefabs/Giant01 - Golem.prefab";
    private const string RockMonsterPrefabPath = "Assets/Prefab/Rock Monster.prefab";
    private const string BossPrefabPath = "Assets/Prefab/Boss.prefab";
    private const string ControllerPath = "Assets/Animation/Rock Monster Animator.controller";
    private const string HumanoidIdlePath = "Assets/Animation/RockMonster/Idle.fbx";
    private const string HumanoidWalkPath = "Assets/Animation/RockMonster/Walking.fbx";
    private const string HumanoidRunPath = "Assets/Animation/RockMonster/Running.fbx";
    private const string UrpBodyMaterialPath =
        "Assets/Kevin Iglesias/Characters/Humanoid Giant/Materials/Giant01/Giant01_Texture01/URP/URP - Giant01_Texture01.mat";

    [MenuItem("Planet Scav/Setup Rock Monster")]
    public static void SetupFromMenu()
    {
        SetupAll();
        EditorUtility.DisplayDialog(
            "Rock Monster Setup",
            "Rock Monster humanoid animations and Boss visual are configured.\n\n" +
            "Press Play on Final Level to see the golem idle, walk, run, and stun.",
            "OK");
    }

    [InitializeOnLoadMethod]
    private static void AutoSetupOnLoad()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (!System.IO.File.Exists(GolemPrefabPath))
                return;

            if (!NeedsSetup())
                return;

            SetupAll();
            Debug.Log("Planet Scav: Rock Monster setup completed automatically.");
        };
    }

    private static bool NeedsSetup()
    {
        if (!UsesHumanoidImports())
            return true;

        if (!HumanoidClipsReady())
            return true;

        if (AssetDatabase.LoadAssetAtPath<GameObject>(RockMonsterPrefabPath) == null)
            return true;

        if (!System.IO.File.Exists(BossPrefabPath))
            return false;

        var boss = AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefabPath);
        return boss != null && boss.transform.Find("Rock Monster") == null;
    }

    private static bool UsesHumanoidImports()
    {
        var importer = AssetImporter.GetAtPath(HumanoidIdlePath) as ModelImporter;
        return importer != null && importer.animationType == ModelImporterAnimationType.Human;
    }

    public static void SetupAll()
    {
        EnsureHumanoidClips();
        var controller = BuildController();
        var rockMonsterPrefab = CreateOrUpdateRockMonsterPrefab(controller);
        UpdateBossPrefab(rockMonsterPrefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static bool HumanoidClipsReady()
    {
        return LoadClip(HumanoidIdlePath, "Idle") != null
            && LoadClip(HumanoidWalkPath, "Walking") != null
            && LoadClip(HumanoidRunPath, "Run") != null;
    }

    private static void EnsureHumanoidClips()
    {
        EnsureHumanoidClip("Assets/Animation/Idle.fbx", HumanoidIdlePath, "Idle");
        EnsureHumanoidClip("Assets/Animation/Walking.fbx", HumanoidWalkPath, "Walking");
        EnsureHumanoidClip("Assets/Animation/Running.fbx", HumanoidRunPath, "Run");
    }

    private static void EnsureHumanoidClip(string sourcePath, string destPath, string clipName)
    {
        if (!File.Exists(sourcePath))
            return;

        if (!File.Exists(destPath))
            File.Copy(sourcePath, destPath, true);

        AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceUpdate);
        var importer = AssetImporter.GetAtPath(destPath) as ModelImporter;
        if (importer == null)
            return;

        var changed = false;
        if (importer.animationType != ModelImporterAnimationType.Human)
        {
            importer.animationType = ModelImporterAnimationType.Human;
            changed = true;
        }

        if (importer.avatarSetup != ModelImporterAvatarSetup.NoAvatar)
        {
            importer.avatarSetup = ModelImporterAvatarSetup.NoAvatar;
            changed = true;
        }

        var clips = importer.clipAnimations;
        if (clips == null || clips.Length == 0)
        {
            clips = new[] { new ModelImporterClipAnimation { name = clipName } };
            changed = true;
        }

        foreach (var clip in clips)
        {
            if (clip.name != clipName)
                continue;

            if (!clip.loopTime)
            {
                clip.loopTime = true;
                changed = true;
            }
        }

        if (!changed)
            return;

        importer.clipAnimations = clips;
        importer.SaveAndReimport();
    }

    private static AnimatorController BuildController()
    {
        var idle = LoadClip(HumanoidIdlePath, "Idle");
        var walk = LoadClip(HumanoidWalkPath, "Walking");
        var run = LoadClip(HumanoidRunPath, "Run");

        if (idle == null || walk == null || run == null)
            throw new System.InvalidOperationException("Rock Monster humanoid clips are missing. Reimport Assets/Animation/RockMonster.");

        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
            AssetDatabase.DeleteAsset(ControllerPath);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Stunned", AnimatorControllerParameterType.Bool);

        var root = controller.layers[0].stateMachine;

        var blendTree = new BlendTree
        {
            name = "Locomotion",
            blendType = BlendTreeType.Simple1D,
            blendParameter = "Speed",
            useAutomaticThresholds = false
        };
        blendTree.AddChild(idle, 0f);
        blendTree.AddChild(walk, 0.5f);
        blendTree.AddChild(run, 1f);
        AssetDatabase.AddObjectToAsset(blendTree, controller);

        var locomotion = root.AddState("Locomotion", new Vector3(300f, 0f, 0f));
        locomotion.motion = blendTree;

        var stunned = root.AddState("Stunned", new Vector3(300f, 120f, 0f));
        stunned.motion = idle;

        var toStunned = root.AddAnyStateTransition(stunned);
        toStunned.AddCondition(AnimatorConditionMode.If, 0f, "Stunned");
        toStunned.duration = 0.15f;
        toStunned.canTransitionToSelf = false;

        var toLocomotion = root.AddAnyStateTransition(locomotion);
        toLocomotion.AddCondition(AnimatorConditionMode.IfNot, 0f, "Stunned");
        toLocomotion.duration = 0.2f;
        toLocomotion.canTransitionToSelf = false;

        root.defaultState = locomotion;
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static AnimationClip LoadClip(string path, string clipName)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<AnimationClip>()
            .FirstOrDefault(c => c.name == clipName);
    }

    private static GameObject CreateOrUpdateRockMonsterPrefab(AnimatorController controller)
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(GolemPrefabPath);
        if (source == null)
            throw new System.InvalidOperationException($"Missing Golem prefab at {GolemPrefabPath}");

        var instance = Object.Instantiate(source);
        instance.name = "Rock Monster";

        try
        {
            ApplyUrpMaterials(instance);
            ConfigureVisualAnimator(instance, controller);

            if (AssetDatabase.LoadAssetAtPath<GameObject>(RockMonsterPrefabPath) == null)
                PrefabUtility.SaveAsPrefabAsset(instance, RockMonsterPrefabPath);
            else
                PrefabUtility.SaveAsPrefabAssetAndConnect(instance, RockMonsterPrefabPath, InteractionMode.AutomatedAction);
        }
        finally
        {
            Object.DestroyImmediate(instance);
        }

        return AssetDatabase.LoadAssetAtPath<GameObject>(RockMonsterPrefabPath);
    }

    private static void ApplyUrpMaterials(GameObject root)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(UrpBodyMaterialPath);
        if (material == null)
            return;

        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            renderer.sharedMaterial = material;
    }

    private static void ConfigureVisualAnimator(GameObject root, AnimatorController controller)
    {
        var animator = root.GetComponent<Animator>();
        if (animator == null)
            animator = root.AddComponent<Animator>();

        var avatar = AssetDatabase.LoadAllAssetsAtPath(GolemModelPath).OfType<Avatar>().FirstOrDefault();
        if (avatar != null)
            animator.avatar = avatar;

        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    }

    private static void UpdateBossPrefab(GameObject rockMonsterPrefab)
    {
        if (rockMonsterPrefab == null || !File.Exists(BossPrefabPath))
            return;

        var bossRoot = PrefabUtility.LoadPrefabContents(BossPrefabPath);
        try
        {
            DisableLegacyVisuals(bossRoot.transform);

            var locomotion = bossRoot.GetComponent<RockMonsterLocomotion>();
            if (locomotion == null)
                locomotion = bossRoot.AddComponent<RockMonsterLocomotion>();

            var bossMovement = bossRoot.GetComponent<BossMovement>();
            var bossEnemy = bossRoot.GetComponent<BossEnemy>();
            var agent = bossRoot.GetComponent<NavMeshAgent>();

            RemoveRootAnimator(bossRoot);

            var existingVisual = bossRoot.transform.Find("Rock Monster");
            if (existingVisual != null)
                Object.DestroyImmediate(existingVisual.gameObject);

            var visual = PrefabUtility.InstantiatePrefab(rockMonsterPrefab, bossRoot.transform) as GameObject;
            if (visual != null)
            {
                visual.name = "Rock Monster";
                visual.transform.localPosition = new Vector3(0f, -0.34f, 0f);
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale = Vector3.one * 0.22f;
            }

            var childAnimator = bossRoot.GetComponentInChildren<Animator>(true);
            if (childAnimator != null)
            {
                var so = new SerializedObject(locomotion);
                so.FindProperty("animator").objectReferenceValue = childAnimator;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            if (bossEnemy != null && agent != null)
            {
                var patrolSpeed = bossEnemy.speed > 0f ? bossEnemy.speed : agent.speed;
                var rushSpeed = patrolSpeed * 2.5f;
                locomotion.ConfigureSpeeds(patrolSpeed, rushSpeed);
            }

            if (bossMovement != null)
            {
                var so = new SerializedObject(bossMovement);
                var locomotionProp = so.FindProperty("locomotion");
                if (locomotionProp != null)
                    locomotionProp.objectReferenceValue = locomotion;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(bossRoot, BossPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(bossRoot);
        }
    }

    private static void DisableLegacyVisuals(Transform root)
    {
        foreach (Transform child in root)
        {
            if (child.name == "Rock Monster")
                continue;

            if (child.GetComponentInChildren<SkinnedMeshRenderer>(true) != null ||
                child.name is "EyeCTRL" or "TurtleShell" or "Crown")
                child.gameObject.SetActive(false);
        }
    }

    private static void RemoveRootAnimator(GameObject root)
    {
        var animator = root.GetComponent<Animator>();
        if (animator != null)
            Object.DestroyImmediate(animator);
    }
}
#endif
