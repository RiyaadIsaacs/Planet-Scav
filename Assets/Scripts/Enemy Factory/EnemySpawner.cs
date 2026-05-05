using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject patrolPrefab;
    public GameObject stationaryPrefab;

    [Header("Spawn Anchors")]
    public Transform[] patrolSpawnPoints;
    public Transform[] stationarySpawnPoints;

    private BaseEnemyFactory factory;

    void Start()
    {
        // Initialize factory with the prefabs.
        factory = new EnemyFactory(patrolPrefab, stationaryPrefab);

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
    }
}