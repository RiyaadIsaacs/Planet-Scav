#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PlatformVendorSetup
{
    private const string FinalScenePath = "Assets/Scenes/Final Level.unity";
    private const string VendorModelPath = "Assets/Prefab/Vendorfbx.fbx";
    private const string PlatformName = "TriggerActivatedPlatform";

    [MenuItem("Planet Scav/Setup Platform Vendor In Final Level")]
    public static void SetupInFinalLevel()
    {
        var scene = EditorSceneManager.OpenScene(FinalScenePath, OpenSceneMode.Single);
        var platform = GameObject.Find(PlatformName);
        if (platform == null)
        {
            Debug.LogError($"Could not find '{PlatformName}' in {FinalScenePath}. Place the platform in the scene first.");
            return;
        }

        platform.SetActive(false);

        var existingVendor = GameObject.Find("PlatformAccessVendor");
        NPCInteractable vendorInteractable;
        GameObject vendorRoot;

        if (existingVendor != null)
        {
            vendorRoot = existingVendor;
            vendorInteractable = vendorRoot.GetComponent<NPCInteractable>();
            if (vendorInteractable == null)
                vendorInteractable = vendorRoot.AddComponent<NPCInteractable>();
        }
        else
        {
            var vendorModel = AssetDatabase.LoadAssetAtPath<GameObject>(VendorModelPath);
            if (vendorModel == null)
            {
                Debug.LogError($"Missing vendor model at {VendorModelPath}");
                return;
            }

            vendorRoot = PrefabUtility.InstantiatePrefab(vendorModel, scene) as GameObject;
            if (vendorRoot == null)
            {
                Debug.LogError("Failed to instantiate vendor model.");
                return;
            }

            vendorRoot.name = "PlatformAccessVendor";
            vendorRoot.transform.position = platform.transform.position + new Vector3(-6f, 4f, 4f);
            vendorRoot.transform.rotation = platform.transform.rotation;

            vendorInteractable = vendorRoot.AddComponent<NPCInteractable>();

            var solidCollider = vendorRoot.GetComponent<Collider>();
            if (solidCollider == null)
            {
                solidCollider = vendorRoot.AddComponent<BoxCollider>();
                solidCollider.isTrigger = false;
            }

            var trigger = vendorRoot.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(2.5f, 3f, 2.5f);
            trigger.center = new Vector3(0f, 0.3f, 0f);
        }

        var vendorSo = new SerializedObject(vendorInteractable);
        vendorSo.FindProperty("cost").intValue = 100;
        vendorSo.FindProperty("upgradeName").stringValue = "Secret Platform Access";
        vendorSo.FindProperty("upgradeType").enumValueIndex = (int)VendorUpgradeType.PlatformAccess;
        vendorSo.FindProperty("disableVendorAfterPurchase").boolValue = true;

        var reveal = vendorSo.FindProperty("revealOnPurchase");
        reveal.arraySize = 1;
        reveal.GetArrayElementAtIndex(0).objectReferenceValue = platform;

        vendorSo.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = vendorRoot;
        Debug.Log("Platform vendor setup complete. TriggerActivatedPlatform is hidden until purchase.");
    }
}
#endif
