using UnityEngine;

[CreateAssetMenu(fileName = "DialogueItem", menuName = "Dialogue/Item")]
public class DialogueItem : ScriptableObject
{
    public string alertName;
    public Sprite icon;
    public string textID;
}