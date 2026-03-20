using UnityEngine;

[CreateAssetMenu(fileName = "DialogueItem", menuName = "Scriptable Objects/DialogueItem")]
public class DialogueItem : ScriptableObject
{
    public string alertName;
    public Sprite icon;
    public string textID;
}
