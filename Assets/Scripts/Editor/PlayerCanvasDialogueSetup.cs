#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class PlayerCanvasDialogueSetup
{
    private const string PrefabPath = "Assets/Prefab/UI Canvas/PlayerCanvas.prefab";

    [MenuItem("Planet Scav/Setup PlayerCanvas Dialogue")]
    public static void SetupFromMenu() => Setup();

    public static void ExecuteFromBatch()
    {
        Setup();
        EditorApplication.Exit(0);
    }

    private static void Setup()
    {
        var root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            var canvasRect = root.GetComponent<RectTransform>();

            DestroyChildIfExists(canvasRect, "DialoguePanel");
            DestroyChildIfExists(canvasRect, "InteractPrompt");
            DestroyChildIfExists(canvasRect, "ChargePanel");

            var dialoguePanel = CreateDialoguePanel(canvasRect);
            var interactPrompt = CreateInteractPrompt(canvasRect);
            var chargePanel = CreateChargePanel(canvasRect);

            var dialogueUI = root.GetComponent<DialogueUIManager>();
            if (dialogueUI == null)
                dialogueUI = root.AddComponent<DialogueUIManager>();

            WireDialogueUIManager(dialogueUI, dialoguePanel, interactPrompt, chargePanel);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        WirePlayerPrefabDialogueReference();

        AssetDatabase.SaveAssets();
        Debug.Log("PlayerCanvas dialogue UI setup complete.");
    }

    private static void WirePlayerPrefabDialogueReference()
    {
        const string playerPath = "Assets/Prefab/Player.prefab";
        var playerRoot = PrefabUtility.LoadPrefabContents(playerPath);
        try
        {
            var uiManager = playerRoot.GetComponent<UIManager>();
            if (uiManager == null)
                return;

            var dialogueUI = playerRoot.GetComponentInChildren<DialogueUIManager>(true);
            if (dialogueUI == null)
                return;

            var so = new SerializedObject(uiManager);
            so.FindProperty("dialogueUI").objectReferenceValue = dialogueUI;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(playerRoot, playerPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(playerRoot);
        }
    }

    private static void DestroyChildIfExists(Transform parent, string childName)
    {
        var child = parent.Find(childName);
        if (child != null)
            Object.DestroyImmediate(child.gameObject);
    }

    private static GameObject CreateDialoguePanel(RectTransform parent)
    {
        var panel = CreateUIObject("DialoguePanel", parent);
        panel.SetActive(false);

        var panelRect = panel.GetComponent<RectTransform>();
        StretchFull(panelRect);

        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.75f);
        bg.raycastTarget = true;

        var alertName = CreateTMPText("AlertName", panelRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0, -40), new Vector2(700, 50), 28, FontStyles.Bold, "Alert");
        var message = CreateTMPText("Message", panelRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(800, 300), 22, FontStyles.Normal, "Dialogue message");
        message.alignment = TextAlignmentOptions.TopLeft;

        var iconGo = CreateUIObject("Icon", panelRect);
        var iconRect = iconGo.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(80, 0);
        iconRect.sizeDelta = new Vector2(96, 96);
        var iconImage = iconGo.AddComponent<Image>();
        iconImage.raycastTarget = false;
        iconImage.enabled = false;

        var nextBtn = CreateButton("NextButton", panelRect, new Vector2(0.5f, 0f), new Vector2(160, 44),
            new Vector2(0, 40), "Next");

        panel.GetComponent<RectTransform>().SetAsLastSibling();
        return panel;
    }

    private static GameObject CreateInteractPrompt(RectTransform parent)
    {
        var promptGo = CreateUIObject("InteractPrompt", parent);
        promptGo.SetActive(false);

        var rect = promptGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0, 80);
        rect.sizeDelta = new Vector2(400, 40);

        var text = promptGo.AddComponent<TextMeshProUGUI>();
        ApplyDefaultFont(text);
        text.fontSize = 20;
        text.alignment = TextAlignmentOptions.Center;
        text.text = "Press E to interact";

        return promptGo;
    }

    private static GameObject CreateChargePanel(RectTransform parent)
    {
        var panel = CreateUIObject("ChargePanel", parent);
        panel.SetActive(false);

        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = new Vector2(-40f, -40f);
        panelRect.sizeDelta = new Vector2(130f, 360f);

        var panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.7f);
        panelBg.raycastTarget = false;

        var label = CreateTMPText("ChargeLabel", panelRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(0f, -20f), new Vector2(120f, 30f), 18, FontStyles.Bold, "Pump To Charge");
        label.color = new Color(1f, 0.8f, 0f);
        label.alignment = TextAlignmentOptions.Center;

        var backgroundGo = CreateUIObject("ChargeBackground", panelRect);
        var backgroundRect = backgroundGo.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
        backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
        backgroundRect.anchoredPosition = new Vector2(0f, 10f);
        backgroundRect.sizeDelta = new Vector2(60f, 220f);
        var backgroundImage = backgroundGo.AddComponent<Image>();
        backgroundImage.color = new Color(1f, 1f, 1f, 0.15f);
        backgroundImage.raycastTarget = false;

        var fillGo = CreateUIObject("ChargeFill", backgroundRect);
        var fillRect = fillGo.GetComponent<RectTransform>();
        StretchFull(fillRect);
        var fillImage = fillGo.AddComponent<Image>();
        fillImage.color = new Color(0.4f, 1f, 0.6f);
        fillImage.raycastTarget = false;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Vertical;
        fillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
        fillImage.fillAmount = 0f;

        var valueText = CreateTMPText("ChargeValue", panelRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 30f), new Vector2(120f, 30f), 18, FontStyles.Bold, "0.0 / 30");
        valueText.alignment = TextAlignmentOptions.Center;

        return panel;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static TextMeshProUGUI CreateTMPText(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPos, Vector2 size, float fontSize, FontStyles style, string defaultText)
    {
        var go = CreateUIObject(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        var text = go.AddComponent<TextMeshProUGUI>();
        ApplyDefaultFont(text);
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.text = defaultText;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(string name, RectTransform parent, Vector2 anchor, Vector2 size,
        Vector2 anchoredPos, string label)
    {
        var go = CreateUIObject(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        var image = go.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        var button = go.AddComponent<Button>();
        button.targetGraphic = image;

        var labelGo = CreateUIObject("Text", rect);
        StretchFull(labelGo.GetComponent<RectTransform>());
        var labelText = labelGo.AddComponent<TextMeshProUGUI>();
        ApplyDefaultFont(labelText);
        labelText.fontSize = 20;
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.text = label;
        labelText.raycastTarget = false;

        return button;
    }

    private static void ApplyDefaultFont(TextMeshProUGUI text)
    {
        var font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font != null)
            text.font = font;
    }

    private static void WireDialogueUIManager(DialogueUIManager dialogueUI, GameObject dialoguePanel,
        GameObject interactPrompt, GameObject chargePanel)
    {
        var so = new SerializedObject(dialogueUI);
        so.FindProperty("dialoguePanel").objectReferenceValue = dialoguePanel;
        so.FindProperty("alertNameText").objectReferenceValue = dialoguePanel.transform.Find("AlertName")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("messageText").objectReferenceValue = dialoguePanel.transform.Find("Message")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("iconImage").objectReferenceValue = dialoguePanel.transform.Find("Icon")?.GetComponent<Image>();
        so.FindProperty("nextButton").objectReferenceValue = dialoguePanel.transform.Find("NextButton")?.GetComponent<Button>();
        so.FindProperty("interactPromptText").objectReferenceValue = interactPrompt.GetComponent<TextMeshProUGUI>();
        so.FindProperty("messageAlert").objectReferenceValue =
            dialoguePanel.transform.parent?.Find("Message Alert")?.gameObject;
        so.FindProperty("chargePanel").objectReferenceValue = chargePanel;
        so.FindProperty("chargeFillImage").objectReferenceValue =
            chargePanel.transform.Find("ChargeBackground/ChargeFill")?.GetComponent<Image>();
        so.FindProperty("chargeValueText").objectReferenceValue =
            chargePanel.transform.Find("ChargeValue")?.GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif
