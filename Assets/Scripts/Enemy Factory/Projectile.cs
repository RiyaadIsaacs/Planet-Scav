using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifetimeSeconds = 6f;
    [SerializeField] private Vector3 velocity;

    private float aliveFor;

    public void SetVelocity(Vector3 v) => velocity = v;
    public void SetLifetime(float seconds) => lifetimeSeconds = seconds;

    private void Update()
    {
        transform.position += velocity * Time.deltaTime;

        aliveFor += Time.deltaTime;
        if (aliveFor >= lifetimeSeconds)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var death = FindFirstObjectByType<DeathHandling>();
            if (death != null)
                death.KillPlayer();
            else
                Debug.LogWarning("Projectile hit Player but no DeathHandling found.");

            Destroy(gameObject);
        }
    }
}

