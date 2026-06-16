using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIE_Button = UnityEngine.UI.Button;
using UIE_Image = UnityEngine.UI.Image;
using UIE_Label = UnityEngine.UIElements.Label;
using UIE_UIDocument = UnityEngine.UIElements.UIDocument;
using UIE_VisualElement = UnityEngine.UIElements.VisualElement;
using DisplayStyle = UnityEngine.UIElements.DisplayStyle;
using Length = UnityEngine.UIElements.Length;
using LengthUnit = UnityEngine.UIElements.LengthUnit;

public class DialogueUIManager : MonoBehaviour
{
    [Header("Dialogue Data")]
    public DialogueSequence sequence;
    public string localizationFile = "Beginner";

    [Header("Player Reference")]
    [SerializeField] private PlayerController playerController;

    [Header("Dialogue Panel (uGUI on Player Canvas)")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text alertNameText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private UIE_Image iconImage;
    [SerializeField] private UIE_Button nextButton;
    [SerializeField] private TMP_Text interactPromptText;

    [Header("New Message Indicator")]
    [SerializeField] private GameObject messageAlert;

    [Header("Charge Bar (optional uGUI)")]
    [SerializeField] private GameObject chargePanel;
    [SerializeField] private UIE_Image chargeFillImage;
    [SerializeField] private TMP_Text chargeValueText;

    private UIE_VisualElement chargeContainer;
    private UIE_VisualElement chargeFill;
    private UIE_Label chargeValueLabel;

    private DialogueQueue queue = new DialogueQueue();
    private DialogueItem currentItem;
    private bool hasCurrentLine;
    private bool panelVisible;
    private bool dialogueCausedPause;

    public bool IsDialoguePanelVisible => panelVisible;

    private bool levelConfigured;

    private void Awake()
    {
        if (messageAlert == null)
            messageAlert = transform.Find("Message Alert")?.gameObject;

        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    public void ConfigureForLevel(PlayerController controller, DialogueSequence levelSequence, string levelLocalization)
    {
        if (controller != null)
            playerController = controller;

        if (levelSequence != null)
            sequence = levelSequence;

        if (!string.IsNullOrEmpty(levelLocalization))
            localizationFile = levelLocalization;

        if (sequence == null)
        {
            Debug.LogWarning("DialogueUIManager: no dialogue sequence for this level.");
            return;
        }

        LocalizationManager.LoadLanguage(localizationFile);

        queue = new DialogueQueue();
        foreach (DialogueItem item in sequence.dialogues)
            queue.Enqueue(item);

        LoadFirstLineHidden();
        levelConfigured = true;
    }

    private void Start()
    {
        if (levelConfigured || sequence == null)
            return;

        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        ConfigureForLevel(playerController, sequence, localizationFile);
    }

    private void Update()
    {
        UpdateChargeBar();
    }

    private void LoadFirstLineHidden()
    {
        PrepareCurrentLineHidden();
        RefreshMessageAlert();
        if (interactPromptText != null)
            interactPromptText.gameObject.SetActive(false);
    }

    private void OnNextButtonClicked()
    {
        ShowNextDialogue();
    }

    public void ToggleDialoguePanel()
    {
        if (!hasCurrentLine && !queue.IsEmpty())
        {
            currentItem = queue.Dequeue();
            hasCurrentLine = true;
            ApplyCurrentLineToUI();
        }

        if (!hasCurrentLine)
            return;

        if (panelVisible)
            HideDialoguePanel();
        else
            ShowDialoguePanel();
    }

    public void ShowNextDialogue()
    {
        if (queue.IsEmpty())
        {
            hasCurrentLine = false;
            HideDialoguePanel();
            RefreshMessageAlert();
            return;
        }

        currentItem = queue.Dequeue();
        hasCurrentLine = true;
        ApplyCurrentLineToUI();
        ShowDialoguePanel();
    }

    private void ApplyCurrentLineToUI()
    {
        if (alertNameText != null)
            alertNameText.text = currentItem.alertName;

        if (messageText != null)
            messageText.text = LocalizationManager.GetText(currentItem.textID);

        if (iconImage != null)
        {
            if (currentItem.icon != null)
            {
                iconImage.sprite = currentItem.icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }
        }
    }

    private void ShowDialoguePanel()
    {
        panelVisible = true;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowMessageAlert(false);
        ApplyDialoguePause(true);
    }

    private void HideDialoguePanel(bool restoreGameplay = true)
    {
        panelVisible = false;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (restoreGameplay)
        {
            ApplyDialoguePause(false);
            RefreshMessageAlert();
        }
        else
            dialogueCausedPause = false;
    }

    private void ApplyDialoguePause(bool pause)
    {
        if (pause)
        {
            if (Time.timeScale <= 0f)
                return;

            Time.timeScale = 0f;
            dialogueCausedPause = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        if (!dialogueCausedPause)
            return;

        Time.timeScale = 1f;
        dialogueCausedPause = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void StartNewDialogue(DialogueSequence newSequence, string newLocalizationFile)
    {
        if (newSequence == null)
        {
            Debug.LogError("Cannot start dialogue: sequence is null.");
            return;
        }

        queue = new DialogueQueue();

        if (!string.IsNullOrEmpty(newLocalizationFile))
            LocalizationManager.LoadLanguage(newLocalizationFile);

        foreach (DialogueItem item in newSequence.dialogues)
            queue.Enqueue(item);

        PrepareCurrentLineHidden();
        RefreshMessageAlert();
    }

    public void ShowMessageAlert(bool show)
    {
        if (messageAlert != null)
            messageAlert.SetActive(show);
    }

    private void RefreshMessageAlert()
    {
        ShowMessageAlert(!panelVisible && (hasCurrentLine || !queue.IsEmpty()));
    }

    private void PrepareCurrentLineHidden()
    {
        hasCurrentLine = false;
        currentItem = null;

        if (!queue.IsEmpty())
        {
            currentItem = queue.Dequeue();
            hasCurrentLine = true;
            ApplyCurrentLineToUI();
        }

        HideDialoguePanel(restoreGameplay: false);
    }

    public void ShowInteractPrompt(bool show)
    {
        if (interactPromptText != null)
            interactPromptText.gameObject.SetActive(show);
    }

    public void SetHudOverlayActive(bool active)
    {
        if (!active)
        {
            HideDialoguePanel(restoreGameplay: false);
            ShowInteractPrompt(false);
            ShowMessageAlert(false);
            if (chargePanel != null)
                chargePanel.SetActive(false);
            return;
        }

        RefreshMessageAlert();
    }

    private void UpdateChargeBar()
    {
        if (playerController == null)
            return;

        bool showCharge = playerController.GetCtrlHeld();

        if (chargePanel != null)
            chargePanel.SetActive(showCharge);

        if (chargeContainer != null)
            chargeContainer.style.display = showCharge ? DisplayStyle.Flex : DisplayStyle.None;

        if (!showCharge)
            return;

        float currentCharge = playerController.GetCharge();
        float maxCharge = playerController.GetMaxCharge();
        float percentage = maxCharge > 0 ? currentCharge / maxCharge : 0f;

        if (chargeValueText != null)
            chargeValueText.text = $"{currentCharge:F1} / {maxCharge}";

        if (chargeValueLabel != null)
            chargeValueLabel.text = $"{currentCharge:F1} / {maxCharge}";

        if (chargeFillImage != null)
        {
            chargeFillImage.fillAmount = percentage;
            if (percentage > 0.8f)
                chargeFillImage.color = new Color(1f, 0.3f, 0.3f);
            else if (percentage > 0.5f)
                chargeFillImage.color = new Color(1f, 0.85f, 0.2f);
            else
                chargeFillImage.color = new Color(0.4f, 1f, 0.6f);
        }

        if (chargeFill != null)
        {
            chargeFill.style.height = new Length(percentage * 100f, LengthUnit.Percent);
            if (percentage > 0.8f)
                chargeFill.style.backgroundColor = new Color(1f, 0.3f, 0.3f);
            else if (percentage > 0.5f)
                chargeFill.style.backgroundColor = new Color(1f, 0.85f, 0.2f);
            else
                chargeFill.style.backgroundColor = new Color(0.4f, 1f, 0.6f);
        }
    }
}
