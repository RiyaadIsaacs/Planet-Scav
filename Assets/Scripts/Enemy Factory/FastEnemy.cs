using UnityEngine;
using UnityEngine.AI;

// A faster patrolling variant.
public class FastEnemy : AIEnemy
{
    [SerializeField] private float speedMultiplier = 1.6f;

    public override void Initialize()
    {
        enemyName = "Sprinter";

        // Make sure this type is actually faster even if the factory already randomized speed.
        speed *= speedMultiplier;

        if (TryGetComponent<NavMeshAgent>(out var agent))
        {
            agent.speed = speed;
        }
    }
}

