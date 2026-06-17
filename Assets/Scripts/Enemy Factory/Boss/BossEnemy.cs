using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-10)]
public class BossEnemy : AIEnemy
{
    [SerializeField] private float destroyDelay = 2f;

    private BossMovement bossMovement;
    private RockMonsterLocomotion locomotion;
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
        TryGetComponent(out locomotion);
        TryGetComponent(out agent);

        if (bossMovement == null)
            return;

        // BossMovement patrol/rush speeds are the source of truth (Inspector values on that component).
        speed = bossMovement.PatrolSpeed;
        locomotion?.ConfigureSpeeds(bossMovement.PatrolSpeed, bossMovement.RushSpeed);

        if (agent != null)
        {
            agent.acceleration = 1000f;
            agent.autoBraking = false;
            agent.stoppingDistance = 0f;
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

        EventHandler.OnBossDefeated?.Invoke();

        Destroy(gameObject, destroyDelay);
    }
}
