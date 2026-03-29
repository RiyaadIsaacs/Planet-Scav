using JetBrains.Annotations;
using System.Collections;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class DeathHandling : MonoBehaviour
{
    public PlayerStats player;
    //public Transform playerTrans; 
    public PlayerCheckpoints checkPoints;
    public Canvas deathCanvas;

    public RawImage healthThird;
    public RawImage healthSecond;
    public RawImage healthFirst;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.playerTrans.SetPositionAndRotation(checkPoints._initialPosition, checkPoints._initialRotation);
            HPLossHandler();
        }
    }

    public void HPLossHandler()
    {
        player.health += -1;
        Debug.Log(player.health);

        if (player.health == 2)
        {
            healthThird.gameObject.SetActive(false);
        }
        else if (player.health == 1)
        {
            healthSecond.gameObject.SetActive(false);
        }

        if (player.health <= 0)
        {
            healthFirst.gameObject.SetActive(false);
            Debug.Log("Player has died.");
            deathCanvas.gameObject.SetActive(true); // Show the death canvas
            Time.timeScale = 0f; // Pause the game
        }
    }

    public void HPGainHandler()
    {
        if (player.health == 3)
        {
            healthThird.gameObject.SetActive(true);
            healthSecond.gameObject.SetActive(true);
            healthFirst.gameObject.SetActive(true);
        }
        else if (player.health == 2)
        {
            healthSecond.gameObject.SetActive(true);
            healthFirst.gameObject.SetActive(true);
        }
        else if (player.health == 1)
        {
            healthFirst.gameObject.SetActive(true);
        }
    }

}
