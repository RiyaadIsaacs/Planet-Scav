using UnityEngine;

// Base class for AI-controlled enemies in the game.
public abstract class AIEnemy : MonoBehaviour
{
    [Header("Base Settings")]
    // Enemy's health
    public float health; 
    public float maxHealth;

    public float speed; // Enemy's movement speed.
    public string enemyName; // Name of the enemy.

    // called by the factory after spawning.
    public abstract void Initialize();

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Handle collision/death handling.
            Debug.Log($"{enemyName} collided with the player!");
        }
    }
}
