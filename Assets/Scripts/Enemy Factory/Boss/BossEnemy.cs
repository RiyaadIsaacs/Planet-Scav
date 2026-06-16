using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-10)]
public class BossEnemy : AIEnemy
{
    [SerializeField] private float _rushSpeedMultiplier = 2.5f;

    private void Awake()
    {
        Initialize();
    }

    public override void Initialize()
    {
        enemyName = "Boss";

        if (!TryGetComponent<BossMovement>(out var bossMovement))
            return;

        var rushSpeed = speed * _rushSpeedMultiplier;
        bossMovement.ConfigureMovement(speed, rushSpeed);

        if (TryGetComponent<NavMeshAgent>(out var agent))
            agent.speed = speed;
    }
}
