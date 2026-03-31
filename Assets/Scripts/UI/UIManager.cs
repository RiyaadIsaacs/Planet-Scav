using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [Header("Dialogue Data")]
    public DialogueSequence sequence;
    public string localizationFile = "Beginner";   // Changes dialog per scene. 

    [Header("Player Reference")]
    [SerializeField] private PlayerController playerController;

    [Header("UI Toolkit")]
    [SerializeField] private UIDocument dialogueUIDocument;

    // References to UI elements for displaying. 
    private VisualElement root;
    private VisualElement iconElement;
    private Label alertNameElement;
    private Label messageElement;
    private Button nextButton;

    // HUD (Charge Bar) elements
    private VisualElement chargeContainer;
    private VisualElement chargeFill;
    private Label chargeValueLabel;

    private DialogueQueue queue = new DialogueQueue(); // Custom queue to manage dialogue items.

    void Start()
    {
        if (dialogueUIDocument == null)
        {
            Debug.LogError("UIDocument is not assigned. pls fix");
            return;
        }

        if (sequence == null)
        {
            Debug.LogError("DialogueSequence is not assigned. should fix");
            return;
        }

        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
            if (playerController == null)
                Debug.LogWarning("PlayerController not found in scene. Where did he go?");
        }

        // Setup UI references.
        root = dialogueUIDocument.rootVisualElement;

        // find the UI elements by name.
        iconElement = root.Q<VisualElement>("icon");
        alertNameElement = root.Q<Label>("alert-name");
        messageElement = root.Q<Label>("message");
        nextButton = root.Q<Button>("next-button");

        // Charge bar elements by name.
        chargeContainer = root.Q<VisualElement>("charge-container");
        chargeFill = root.Q<VisualElement>("charge-fill");
        chargeValueLabel = root.Q<Label>("charge-value");

        // Subscribing to the next button.
        if (nextButton != null)
            nextButton.clicked += ShowNextDialogue;

        // Load the JSON for this level
        LocalizationManager.LoadLanguage(localizationFile);

        // Fill the queue from the sequence
        foreach (DialogueItem item in sequence.dialogues)
        {
            queue.Enqueue(item);
        }

        ShowNextDialogue();
    }

    void Update()
    {
        // Update charge bar every frame
        UpdateChargeBar();
    }

    private void UpdateChargeBar()
    {
        // safety check.
        if (playerController == null || chargeFill == null || chargeValueLabel == null)
            return;

        ChargeBarVisibility();

        // Get current charge and max charge 
        float currentCharge = playerController.GetCharge();
        float maxCharge = playerController.GetMaxCharge();

        float percentage = maxCharge > 0 ? (currentCharge / maxCharge) * 100f : 0f; // return percentage if >0, else return 0.

        // fill charge bar vertically based on perventage.
        chargeFill.style.height = new Length(percentage, LengthUnit.Percent);

        // Update text
        chargeValueLabel.text = $"{currentCharge:F1} / {maxCharge}";

        // Color changes based on charge.
        if (percentage > 80f)
            chargeFill.style.backgroundColor = new Color(1f, 0.3f, 0.3f);     // Red (high)
        else if (percentage > 50f)
            chargeFill.style.backgroundColor = new Color(1f, 0.85f, 0.2f);   // Yellow
        else
            chargeFill.style.backgroundColor = new Color(0.4f, 1f, 0.6f);    // Green
    }

    private void ChargeBarVisibility()
    {
        bool isHoldingCtrl = playerController.GetCtrlHeld();

        // Show charge bar only when holding Ctrl
        if (isHoldingCtrl)
        {
            chargeContainer.style.display = DisplayStyle.Flex;
        }
        else
        {
            chargeContainer.style.display = DisplayStyle.None;
        }
    }

    public void ShowNextDialogue()
    {
        // hide the dialogue box cause no more dialogue. 
        if (queue.IsEmpty())
        {
            HideDialogueBox();
            return;
        }

        ShowDialogueBox();

        DialogueItem item = queue.Dequeue();

        // Update the dialogue UI.
        if (alertNameElement != null) alertNameElement.text = item.alertName;
        if (messageElement != null) messageElement.text = LocalizationManager.GetText(item.textID);
        
        if (iconElement != null && item.icon != null)
        {
            iconElement.style.backgroundImage = new StyleBackground(item.icon);
        }

        if (nextButton != null)
            nextButton.text = "Next";
    }

    // Makes the whole dialogue visible.
    private void ShowDialogueBox()
    {
        if (root != null)
            root.style.display = DisplayStyle.Flex;     
    }

    // Makes the whole dialogue invisible.
    private void HideDialogueBox()
    {
        if (root != null)
            root.style.display = DisplayStyle.None;
    }

    // Allows starting a new dialogue sequence at runtime.
    public void StartNewDialogue(DialogueSequence newSequence, string newLocalizationFile)
    {
        if (newSequence == null)
        {
            Debug.LogError("Cannot start dialogue, you lost the sequence");
            return;
        }

        // Clear the old queue.
        queue = new DialogueQueue();

        // Load level localization file just in case.
        if (!string.IsNullOrEmpty(newLocalizationFile))
        {
            LocalizationManager.LoadLanguage(newLocalizationFile);
        }

        // Fill the queue with the new dialogue items.
        foreach (DialogueItem item in newSequence.dialogues)
        {
            queue.Enqueue(item);
        }

        // Show the dialogue box and display first item.
        ShowNextDialogue();
    }
}