using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallingStalactite : MonoBehaviour
{
    [SerializeField] private float fallDamageToBoss = 50f;
    [SerializeField] private bool startHanging = true;

    private Rigidbody rb;
    private bool isFalling;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (startHanging)
            SetHanging();
    }

    public void TriggerFall()
    {
        BeginFall();
    }

    public void TriggerFall(PlayerProjectile source)
    {
        BeginFall();
    }

    private void BeginFall()
    {
        if (isFalling)
            return;

        isFalling = true;
        transform.SetParent(null);
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isFalling)
            return;

        if (other.TryGetComponent<PlayerProjectile>(out _))
            TriggerFall(null);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isFalling)
            return;

        if (collision.collider.CompareTag("Player"))
        {
            var death = collision.collider.GetComponentInParent<PlayerController>()?.deathHandling
                        ?? FindFirstObjectByType<DeathHandling>();
            if (death != null)
                death.KillPlayer();
            return;
        }

        var enemy = collision.collider.GetComponentInParent<AIEnemy>();
        if (enemy != null && !enemy.isDead)
        {
            if (enemy is BossEnemy boss)
                boss.TakeDamage(fallDamageToBoss);
            else
                enemy.Die();

            Destroy(gameObject);
        }
    }

    private void SetHanging()
    {
        isFalling = false;
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
