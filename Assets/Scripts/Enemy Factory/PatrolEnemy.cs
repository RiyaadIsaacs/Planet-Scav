using UnityEngine;
using UnityEngine.AI;

// Patrolling enemy
public class PatrolEnemy : AIEnemy
{
    public override void Initialize()
    {
        enemyName = "Patroller";

        // Apply factory-assigned speed to the NavMeshAgent.
        if (TryGetComponent<NavMeshAgent>(out var agent))
        {
            agent.speed = speed;
        }
    }
}

