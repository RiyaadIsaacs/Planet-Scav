using UnityEngine;

[RequireComponent(typeof(BossEnemy))]
public class BossShooting : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireInterval = 5f;
    [SerializeField] private float projectileSpeed = 14f;
    [SerializeField] private float projectileLifetime = 8f;
    [SerializeField] private float fireHeightOffset = 1.8f;
    [SerializeField] private float aimHeightOffset = 1f;

    [Header("Activation")]
    [SerializeField] private bool requireCheckpoint = true;
    [SerializeField] private string requiredCheckpointName = "Checkpoint (4)";

    private BossEnemy boss;
    private Transform playerTarget;
    private PlayerCheckpoints playerCheckpoints;
    private float nextFireTime;
    private bool fireTimerStarted;
    private bool shootingEnabled;

    private void Awake()
    {
        boss = GetComponent<BossEnemy>();
    }

    private void OnEnable()
    {
        PlayerCheckpoints.CheckpointReached += HandleCheckpointReached;
    }

    private void OnDisable()
    {
        PlayerCheckpoints.CheckpointReached -= HandleCheckpointReached;
    }

    private void Update()
    {
        if (boss != null && boss.isDead)
            return;

        if (projectilePrefab == null)
            return;

        if (!TryResolvePlayerTarget())
            return;

        if (!IsShootingEnabled())
            return;

        if (!fireTimerStarted)
        {
            nextFireTime = Time.time + fireInterval;
            fireTimerStarted = true;
        }

        if (Time.time < nextFireTime)
            return;

        FireAtPlayer();
        nextFireTime = Time.time + fireInterval;
    }

    private bool IsShootingEnabled()
    {
        if (!requireCheckpoint)
            return true;

        if (shootingEnabled)
            return true;

        TryEnableFromCurrentCheckpoint();
        return shootingEnabled;
    }

    private void HandleCheckpointReached(Transform checkpoint)
    {
        if (!requireCheckpoint || checkpoint == null)
            return;

        if (checkpoint.name == requiredCheckpointName)
            shootingEnabled = true;
    }

    private void TryEnableFromCurrentCheckpoint()
    {
        if (shootingEnabled)
            return;

        if (playerCheckpoints == null && playerTarget != null)
            playerCheckpoints = playerTarget.GetComponentInParent<PlayerCheckpoints>();

        var currentCheckpoint = playerCheckpoints?.PeekRespawn();
        if (currentCheckpoint != null && currentCheckpoint.name == requiredCheckpointName)
            shootingEnabled = true;
    }

    private bool TryResolvePlayerTarget()
    {
        if (playerTarget != null)
            return true;

        if (GameSession.Instance != null && GameSession.Instance.Player != null)
        {
            playerTarget = GameSession.Instance.Player.transform;
            return true;
        }

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTarget = player.transform;
            return true;
        }

        var controller = FindFirstObjectByType<PlayerController>();
        if (controller != null)
        {
            playerTarget = controller.transform;
            return true;
        }

        return false;
    }

    private void FireAtPlayer()
    {
        var origin = firePoint != null
            ? firePoint.position
            : transform.position + Vector3.up * fireHeightOffset;

        var aimPoint = playerTarget.position + Vector3.up * aimHeightOffset;
        var direction = aimPoint - origin;
        if (direction.sqrMagnitude < 0.01f)
            return;

        direction.Normalize();

        var projectile = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(direction));

        if (projectile.TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = direction * projectileSpeed;

        if (projectile.TryGetComponent<Projectile>(out var projectileLogic))
        {
            projectileLogic.SetVelocity(direction * projectileSpeed);
            projectileLogic.SetLifetime(projectileLifetime);
        }
        else
        {
            Destroy(projectile, projectileLifetime);
        }
    }
}
