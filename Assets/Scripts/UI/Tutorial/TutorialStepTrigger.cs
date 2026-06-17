using UnityEngine;

/// <summary>
/// Shows a tutorial canvas when the player enters this zone (once).
/// </summary>
[RequireComponent(typeof(Collider))]
public class TutorialStepTrigger : MonoBehaviour
{
    [SerializeField] private TutorialUIManager tutorialUI;
    [SerializeField] private int stepIndex;
    [SerializeField] private bool onlyTriggerOnce = true;

    private bool hasTriggered;

    private void Reset()
    {
        var collider = GetComponent<Collider>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (onlyTriggerOnce && hasTriggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (tutorialUI == null)
            tutorialUI = FindFirstObjectByType<TutorialUIManager>();

        if (tutorialUI == null)
        {
            Debug.LogWarning($"TutorialStepTrigger on {name}: no TutorialUIManager found.", this);
            return;
        }

        tutorialUI.ShowStep(stepIndex);
        hasTriggered = true;
    }
}
