using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Shows one of five tutorial canvases at a time and pauses gameplay while a message is visible.
/// </summary>
public class TutorialUIManager : MonoBehaviour
{
    [System.Serializable]
    public struct TutorialStep
    {
        public Canvas canvas;
        public TMP_Text titleText;
        public TMP_Text messageText;
        public string title;
        [TextArea(2, 5)]
        public string message;
    }

    [SerializeField] private TutorialStep[] steps;
    [SerializeField] private bool showFirstStepOnStart = true;
    [SerializeField] private float firstStepDelay = 0.75f;

    private int activeStep = -1;
    private float firstStepTimer;

    private void Awake()
    {
        WireDismissButtons();
    }

    private void Start()
    {
        HideAllSteps();

        if (showFirstStepOnStart && steps != null && steps.Length > 0)
            firstStepTimer = firstStepDelay;
    }

    private void Update()
    {
        if (firstStepTimer > 0f)
        {
            firstStepTimer -= Time.unscaledDeltaTime;
            if (firstStepTimer <= 0f)
                ShowStep(0);
        }

        if (activeStep >= 0 && WasDismissPressed())
            HideCurrentStep();
    }

    private static bool WasDismissPressed()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return false;

        return keyboard.enterKey.wasPressedThisFrame
            || keyboard.spaceKey.wasPressedThisFrame
            || keyboard.escapeKey.wasPressedThisFrame;
    }

    private void WireDismissButtons()
    {
        if (steps == null)
            return;

        foreach (var step in steps)
        {
            if (step.canvas == null)
                continue;

            foreach (var button in step.canvas.GetComponentsInChildren<Button>(true))
            {
                if (!button.gameObject.name.Contains("GotIt"))
                    continue;

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(HideCurrentStep);
            }
        }
    }

    public void ShowStep(int index)
    {
        if (steps == null || index < 0 || index >= steps.Length)
            return;

        HideCurrentStep();

        activeStep = index;
        var step = steps[index];

        if (step.titleText != null)
            step.titleText.text = step.title;

        if (step.messageText != null)
            step.messageText.text = step.message;

        if (step.canvas != null)
            step.canvas.gameObject.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HideCurrentStep()
    {
        if (activeStep < 0 || steps == null || activeStep >= steps.Length)
            return;

        if (steps[activeStep].canvas != null)
            steps[activeStep].canvas.gameObject.SetActive(false);

        activeStep = -1;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void HideAllSteps()
    {
        if (steps == null)
            return;

        foreach (var step in steps)
        {
            if (step.canvas != null)
                step.canvas.gameObject.SetActive(false);
        }

        activeStep = -1;
    }
}
