using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;
using UIE_Button = UnityEngine.UI.Button;
using UIE_Image = UnityEngine.UI.Image;

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

    [Header("Charge Bar (optional UI Toolkit — leave empty if using uGUI charge below)")]
    [SerializeField] private UIDocument chargeUIDocument;
    [SerializeField] private int hudSortingOrder = -100;

    [Header("Charge Bar (optional uGUI)")]
    [SerializeField] private GameObject chargePanel;
    [SerializeField] private UIE_Image chargeFillImage;
    [SerializeField] private TMP_Text chargeValueText;

    private VisualElement _chargeContainer;
    private VisualElement _chargeFill;
    private Label _chargeValueLabel;

    private DialogueQueue queue = new DialogueQueue();
    private DialogueItem _currentItem;
    private bool _hasCurrentLine;
    private bool _panelVisible;

    private void Awake()
    {
        if (chargeUIDocument != null && chargeUIDocument.panelSettings != null)
            chargeUIDocument.panelSettings.sortingOrder = hudSortingOrder;

        if (chargeUIDocument != null)
        {
            var root = chargeUIDocument.rootVisualElement;
            _chargeContainer = root.Q<VisualElement>("charge-container");
            _chargeFill = root.Q<VisualElement>("charge-fill");
            _chargeValueLabel = root.Q<Label>("charge-value");
        }
    }

    private void Start()
    {
        if (sequence == null)
        {
            Debug.LogError("DialogueSequence is not assigned.");
            return;
        }

        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);

        LocalizationManager.LoadLanguage(localizationFile);

        queue = new DialogueQueue();
        foreach (DialogueItem item in sequence.dialogues)
            queue.Enqueue(item);

        LoadFirstLineHidden();
    }

    private void Update()
    {
        UpdateChargeBar();
    }

    private void LoadFirstLineHidden()
    {
        if (!queue.IsEmpty())
        {
            _currentItem = queue.Dequeue();
            _hasCurrentLine = true;
            ApplyCurrentLineToUI();
        }

        HideDialoguePanel();
        if (interactPromptText != null)
            interactPromptText.gameObject.SetActive(false);
    }

    private void OnNextButtonClicked()
    {
        ShowNextDialogue();
    }

    public void ToggleDialoguePanel()
    {
        if (!_hasCurrentLine && !queue.IsEmpty())
        {
            _currentItem = queue.Dequeue();
            _hasCurrentLine = true;
            ApplyCurrentLineToUI();
        }

        if (!_hasCurrentLine)
            return;

        _panelVisible = !_panelVisible;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(_panelVisible);
    }

    public void ShowNextDialogue()
    {
        if (queue.IsEmpty())
        {
            _hasCurrentLine = false;
            HideDialoguePanel();
            return;
        }

        _currentItem = queue.Dequeue();
        _hasCurrentLine = true;
        ApplyCurrentLineToUI();
        ShowDialoguePanel();
    }

    private void ApplyCurrentLineToUI()
    {
        if (alertNameText != null)
            alertNameText.text = _currentItem.alertName;

        if (messageText != null)
            messageText.text = LocalizationManager.GetText(_currentItem.textID);

        if (iconImage != null)
        {
            if (_currentItem.icon != null)
            {
                iconImage.sprite = _currentItem.icon;
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
        _panelVisible = true;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
    }

    private void HideDialoguePanel()
    {
        _panelVisible = false;
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
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

        ShowNextDialogue();
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
            HideDialoguePanel();
            ShowInteractPrompt(false);
            if (chargePanel != null)
                chargePanel.SetActive(false);
            if (chargeUIDocument != null)
                chargeUIDocument.enabled = false;
            return;
        }

        if (chargeUIDocument != null)
            chargeUIDocument.enabled = true;
    }

    private void UpdateChargeBar()
    {
        if (playerController == null)
            return;

        bool showCharge = playerController.GetCtrlHeld();

        if (chargePanel != null)
            chargePanel.SetActive(showCharge);

        if (_chargeContainer != null)
            _chargeContainer.style.display = showCharge ? DisplayStyle.Flex : DisplayStyle.None;

        if (!showCharge)
            return;

        float currentCharge = playerController.GetCharge();
        float maxCharge = playerController.GetMaxCharge();
        float percentage = maxCharge > 0 ? currentCharge / maxCharge : 0f;

        if (chargeValueText != null)
            chargeValueText.text = $"{currentCharge:F1} / {maxCharge}";

        if (_chargeValueLabel != null)
            _chargeValueLabel.text = $"{currentCharge:F1} / {maxCharge}";

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

        if (_chargeFill != null)
        {
            _chargeFill.style.height = new Length(percentage * 100f, LengthUnit.Percent);
            if (percentage > 0.8f)
                _chargeFill.style.backgroundColor = new Color(1f, 0.3f, 0.3f);
            else if (percentage > 0.5f)
                _chargeFill.style.backgroundColor = new Color(1f, 0.85f, 0.2f);
            else
                _chargeFill.style.backgroundColor = new Color(0.4f, 1f, 0.6f);
        }
    }
}
