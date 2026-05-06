using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Mode")]
    [Tooltip("If true, spawns using EnemySpawnPoint components in the scene. If false, uses the arrays below.")]
    public bool useSpawnPointComponents = true;

    [Header("Enemy Prefabs")]
    public GameObject patrolPrefab;
    public GameObject stationaryPrefab;
    public GameObject fastPrefab;
    [Tooltip("Prefab fired by Stationary enemies (projectile).")]
    public GameObject stationaryProjectilePrefab;

    [Header("Spawn Anchors")]
    public Transform[] patrolSpawnPoints;
    public Transform[] stationarySpawnPoints;
    public Transform[] fastSpawnPoints;

    private BaseEnemyFactory factory;

    void Start()
    {
        // Initialize factory with the prefabs.
        factory = new EnemyFactory(patrolPrefab, stationaryPrefab, fastPrefab, stationaryProjectilePrefab);

        if (useSpawnPointComponents)
            SpawnFromSpawnPoints();
        else
            SpawnLevelEnemies();
    }

    void SpawnLevelEnemies()
    {
        foreach (Transform point in patrolSpawnPoints)
        {
            factory.CreateEnemy(EEnemyType.EnemyType.Patrol, point.position);
        }

        foreach (Transform point in stationarySpawnPoints)
        {
            factory.CreateEnemy(EEnemyType.EnemyType.Stationary, point.position);
        }

        foreach (Transform point in fastSpawnPoints)
        {
            factory.CreateEnemy(EEnemyType.EnemyType.Fast, point.position);
        }
    }

    void SpawnFromSpawnPoints()
    {
        var points = FindObjectsByType<EnemySpawnPoint>(FindObjectsSortMode.None);
        foreach (var p in points)
        {
            if (p == null) continue;

            var enemy = factory.CreateEnemy(p.type, p.SpawnPosition);
            if (enemy == null) continue;

            // If this spawn point specifies a patrol path, assign it to EnemyMovement.
            if (p.patrolPath != null && enemy.TryGetComponent<EnemyMovement>(out var movement))
            {
                // EnemyMovement._patrolPath is private, so we assign via inspector normally.
                // To keep this lightweight, we only auto-assign if you expose a setter later.
                // For now: ensure your prefab already has the correct PatrolWaypointPath assigned.
            }
        }
    }
}