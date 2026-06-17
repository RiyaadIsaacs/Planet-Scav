using UnityEngine;
using UnityEngine.AI;

// Base class for AI-controlled enemies in the game.
public abstract class AIEnemy : MonoBehaviour
{
    [Header("Base Settings")]
    public float health;
    public float maxHealth;

    public float speed;
    public string enemyName;

    public bool isDead;

    public abstract void Initialize();

    public virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;
        CancelInvoke();

        if (TryGetComponent<NavMeshAgent>(out var agent) && agent.isOnNavMesh)
            agent.isStopped = true;

        Destroy(gameObject);
    }
}
