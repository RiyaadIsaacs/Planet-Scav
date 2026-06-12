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
            KillPlayer();
        }
    }

    public void KillPlayer()
    {
        if (player == null)
        {
            Debug.LogError("DeathHandling: assign the PlayerStats reference in the Inspector (field 'player').");
            return;
        }

        if (player.playerTrans != null && checkPoints != null)
            player.playerTrans.SetPositionAndRotation(checkPoints.initialPosition, checkPoints.initialRotation);

        HPLossHandler();
    }

    public void HPLossHandler()
    {
        if (player == null)
        {
            Debug.LogError("DeathHandling: assign the PlayerStats reference in the Inspector (field 'player').");
            return;
        }

        player.health += -1;
        Debug.Log(player.health);

        if (player.health == 2)
        {
            if (healthThird != null) healthThird.gameObject.SetActive(false);
        }
        else if (player.health == 1)
        {
            if (healthSecond != null) healthSecond.gameObject.SetActive(false);
        }

        if (player.health <= 0)
        {
            if (healthFirst != null) healthFirst.gameObject.SetActive(false);
            Debug.Log("Player has died.");
            if (deathCanvas != null) deathCanvas.gameObject.SetActive(true); // Show the death canvas
            Time.timeScale = 0f; // Pause the game

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            var dialogueUI = FindFirstObjectByType<DialogueUIManager>();
            if (dialogueUI != null)
                dialogueUI.SetHudOverlayActive(false);
        }
    }

    public void HPGainHandler()
    {
        if (player == null) return;

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
