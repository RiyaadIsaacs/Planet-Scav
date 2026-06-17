using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-10)]
public class BossEnemy : AIEnemy
{
    [SerializeField] private float rushSpeedMultiplier = 2.5f;
    [SerializeField] private float destroyDelay = 2f;

    private BossMovement bossMovement;
    private NavMeshAgent agent;

    private void Awake()
    {
        Initialize();
    }

    public override void Initialize()
    {
        enemyName = "Boss";
        if (maxHealth <= 0f)
            maxHealth = health;

        TryGetComponent(out bossMovement);
        TryGetComponent(out agent);

        if (bossMovement == null)
            return;

        var rushSpeed = speed * rushSpeedMultiplier;
        bossMovement.ConfigureMovement(speed, rushSpeed);

        if (agent != null)
        {
            agent.acceleration = 1000f;
            agent.autoBraking = false;
            agent.stoppingDistance = 0f;
            agent.speed = speed;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        health -= amount;
        if (health <= 0f)
            Die();
    }

    public void InstantKill()
    {
        if (isDead)
            return;

        health = 0f;
        Die();
    }

    public override void Die()
    {
        if (isDead)
            return;

        isDead = true;
        CancelInvoke();

        if (bossMovement != null)
            bossMovement.enabled = false;

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = true;

        Destroy(gameObject, destroyDelay);
    }
}
