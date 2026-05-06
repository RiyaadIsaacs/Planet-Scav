using UnityEngine;

public class EnemyFactory : BaseEnemyFactory
{
    // enemy types to spawn.
    private GameObject patrolEnemyPrefab;
    private GameObject stationaryEnemyPrefab;
    private GameObject fastEnemyPrefab;
    private GameObject stationaryProjectilePrefab;

    // Constructor to initialize the enemy prefabs.
    public EnemyFactory(GameObject patrolPrefab, GameObject stationaryPrefab, GameObject fastPrefab, GameObject stationaryProjectilePrefab)
    {
        patrolEnemyPrefab = patrolPrefab;
        stationaryEnemyPrefab = stationaryPrefab;
        fastEnemyPrefab = fastPrefab;
        this.stationaryProjectilePrefab = stationaryProjectilePrefab;
    }

    public override AIEnemy CreateEnemy(EEnemyType.EnemyType type, Vector3 position)
    {
        GameObject enemyPrefab = null; 
        switch (type)
        {
            case EEnemyType.EnemyType.Patrol:
                enemyPrefab = patrolEnemyPrefab;
                break;
            case EEnemyType.EnemyType.Stationary:
                enemyPrefab = stationaryEnemyPrefab;
                break;
            case EEnemyType.EnemyType.Fast:
                enemyPrefab = fastEnemyPrefab;
                break;
            default:
                Debug.LogError("Invalid enemy type specified.");
                return null;
        }
        if (enemyPrefab == null)
        {
            Debug.LogError($"Enemy prefab is null for type {type}.");
            return null;
        }
        GameObject enemyInstance = Object.Instantiate(enemyPrefab, position, Quaternion.identity);
        AIEnemy aiComponent = enemyInstance.GetComponent<AIEnemy>();
        if (aiComponent == null)
        {
            Debug.LogError("Spawned enemy prefab is missing an AIEnemy component.");
            return null;
        }

        // Option C: assign extra dependencies via the factory.
        if (type == EEnemyType.EnemyType.Stationary)
        {
            StationaryEnemy stationary = aiComponent as StationaryEnemy;
            if (stationary != null)
            {
                stationary.projectilePrefab = stationaryProjectilePrefab;
            }
        }

        enemyInstance.transform.localScale *= Random.Range(0.8f, 1.2f); // Size variation
        aiComponent.speed = Random.Range(3f, 6f); // Speed variation

        aiComponent.Initialize();

        return aiComponent;
    }

}
