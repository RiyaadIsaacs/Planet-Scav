using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] public int coinGain = 50;

    public TMP_Text coinText; // Reference to the TextMeshPro component for displaying coins

    public int health = 3; // Player's health
    public Transform playerTrans;
    public int coins = 0; // Player's coin count

    [HideInInspector]
    public bool upgradeCheck = false;

    public void CoinGainHandler()
    {
        coins += coinGain;

        coinText.text = coins.ToString();
    }
}
