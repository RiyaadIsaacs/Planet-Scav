using JetBrains.Annotations;
using System.Collections;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class DeathHandling : MonoBehaviour
{
    public int health = 3; // Player's health
    public Transform player; 
    public PlayerCheckpoints checkPoints;
    public Canvas deathCanvas;

    public RawImage healthThird;
    public RawImage healthSecond;
    public RawImage healthFirst;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.SetPositionAndRotation(checkPoints._initialPosition, checkPoints._initialRotation);
            HPLossHandler();
        }
    }

    public void HPLossHandler()
    {
        health += -1;
        Debug.Log(health);

        if (health == 2)
        {
            healthThird.gameObject.SetActive(false);
        }
        else if (health == 1)
        {
            healthSecond.gameObject.SetActive(false);
        }

        if (health <= 0)
        {
            healthFirst.gameObject.SetActive(false);
            Debug.Log("Player has died.");
            deathCanvas.gameObject.SetActive(true); // Show the death canvas
            Time.timeScale = 0f; // Pause the game
        }
    }

    public void HPGainHandler()
    {
        if (health == 3)
        {
            healthThird.gameObject.SetActive(true);
            healthSecond.gameObject.SetActive(true);
            healthFirst.gameObject.SetActive(true);
        }
        else if (health == 2)
        {
            healthSecond.gameObject.SetActive(true);
            healthFirst.gameObject.SetActive(true);
        }
        else if (health == 1)
        {
            healthFirst.gameObject.SetActive(true);
        }
    }

}
