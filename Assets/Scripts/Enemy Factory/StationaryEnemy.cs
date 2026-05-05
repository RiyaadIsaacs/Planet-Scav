using UnityEngine;

// A stationary enemy that fires projectiles at the player.
public class StationaryEnemy : AIEnemy
{
    public GameObject projectilePrefab;
    public float fireRate = 3f;

    public override void Initialize()
    {
        enemyName = "Sentry";
        InvokeRepeating("FireProjectile", fireRate, fireRate); // used to call the FireProjectile method at a consistent rate.
    }

    void FireProjectile()
    {
        /* Instantiate projectile logic */
    }
}