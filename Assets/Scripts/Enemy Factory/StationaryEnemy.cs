using UnityEngine;

// A stationary enemy that fires projectiles at the player.
public class StationaryEnemy : AIEnemy
{
    public GameObject projectilePrefab;
    public float fireRate = 3f;
    public Transform firePoint;
    public float projectileSpeed = 12f;
    public float projectileLifetime = 6f;

    public override void Initialize()
    {
        enemyName = "Sentry";

        // Default fire point is this transform if none assigned.
        if (firePoint == null)
            firePoint = transform;

        if (projectilePrefab != null)
            InvokeRepeating(nameof(FireProjectile), fireRate, fireRate);
    }

    void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null)
            return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // If the prefab has a Rigidbody, launch it forward.
        if (proj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = firePoint.forward * projectileSpeed;
        }

        // If it has our Projectile script, configure it too.
        if (proj.TryGetComponent<Projectile>(out var projectile))
        {
            projectile.SetVelocity(firePoint.forward * projectileSpeed);
            projectile.SetLifetime(projectileLifetime);
        }
        else
        {
            // Ensure cleanup even if no script is present.
            Destroy(proj, projectileLifetime);
        }
    }
}