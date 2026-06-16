using UnityEngine;

/// <summary>
/// Per-level settings placed in gameplay scenes (on LevelGameplayRoot or similar).
/// </summary>
public class LevelConfig : MonoBehaviour
{
    [Header("Opening Dialogue")]
    public DialogueSequence dialogueSequence;
    public string localizationFile = "Beginner";
}
