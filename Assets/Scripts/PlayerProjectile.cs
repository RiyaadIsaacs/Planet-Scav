using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 30f;
    [SerializeField] private float lifetime = 4f;

    private Vector3 velocity;
    private float damage;
    private bool instantKillBoss;
    private float aliveFor;
    private bool hasHit;
    private Rigidbody rb;

    public void Initialize(Vector3 direction, float bossDamage, bool instantKill)
    {
        damage = bossDamage;
        instantKillBoss = instantKill;
        velocity = direction.normalized * speed;
        transform.rotation = Quaternion.LookRotation(velocity.normalized);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void Update()
    {
        transform.position += velocity * Time.deltaTime;

        aliveFor += Time.deltaTime;
        if (aliveFor >= lifetime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit)
            return;

        if (other.CompareTag("Player") || other.CompareTag("PlayerProjectile"))
            return;

        if (other.TryGetComponent<FallingStalactite>(out var stalactite))
        {
            hasHit = true;
            stalactite.TriggerFall(this);
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Stalactite") &&
            other.TryGetComponent<FallingStalactite>(out stalactite))
        {
            hasHit = true;
            stalactite.TriggerFall(this);
            Destroy(gameObject);
            return;
        }

        var boss = other.GetComponentInParent<BossEnemy>();
        if (boss != null)
        {
            hasHit = true;
            if (instantKillBoss)
                boss.InstantKill();
            else
                boss.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}
