using UnityEngine;
using UnityEngine.UIElements;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue Data")]
    public DialogueSequence sequence;
    public string localizationFile = "Beginner";   // Changes dialog per scene. 

    [Header("UI Toolkit")]
    [SerializeField] private UIDocument dialogueUIDocument;

    // References to UI elements for displaying. 
    private VisualElement root;
    private Image iconElement;
    private Label alertNameElement;
    private Label messageElement;
    private Button nextButton;

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

        // Setup UI references.
        root = dialogueUIDocument.rootVisualElement;

        // find the UI elements by name.
        iconElement = root.Q<Image>("icon");
        alertNameElement = root.Q<Label>("alert-name");
        messageElement = root.Q<Label>("message");
        nextButton = root.Q<Button>("next-button");

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
            iconElement.sprite = item.icon;

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
            Debug.LogError("Cannot start dialogue: Sequence is null");
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