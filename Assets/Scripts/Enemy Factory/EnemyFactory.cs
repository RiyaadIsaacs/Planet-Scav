using UnityEngine;

public class EnemyFactory : BaseEnemyFactory
{
    // enemy types to spawn.
    private GameObject patrolEnemyPrefab;
    private GameObject stationaryEnemyPrefab;

    // Constructor to initialize the enemy prefabs.
    public EnemyFactory(GameObject patrolPrefab, GameObject stationaryPrefab)
    {
        patrolEnemyPrefab = patrolPrefab;
        stationaryEnemyPrefab = stationaryPrefab;
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
            default:
                Debug.LogError("Invalid enemy type specified.");
                return null;
        }
        GameObject enemyInstance = Object.Instantiate(enemyPrefab, position, Quaternion.identity);
        AIEnemy aiComponent = enemyInstance.GetComponent<AIEnemy>();

        enemyInstance.transform.localScale *= Random.Range(0.8f, 1.2f); // Size variation
        aiComponent.speed = Random.Range(3f, 6f); // Speed variation

        return aiComponent;
    }

}
