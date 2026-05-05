using UnityEngine;

// A patrolling enemy that moves back and forth between the points in the linked list.
public class PatrollingEnemy : AIEnemy
{
    public override void Initialize()
    {
        enemyName = "Patroller";
        speed = 2f;
    }
    // Add LinkedList logic under here.
}