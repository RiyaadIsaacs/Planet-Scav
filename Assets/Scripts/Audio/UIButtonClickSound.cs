using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonClickSound : MonoBehaviour
{
    [SerializeField] private string soundId = "click";

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(PlayClick);
    }

    private void PlayClick()
    {
        SFXManager.Play(soundId);
    }
}
