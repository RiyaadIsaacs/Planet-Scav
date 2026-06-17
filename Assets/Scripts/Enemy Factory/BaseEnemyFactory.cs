using UnityEngine;

// Factory pattern to create different types of enemies based on enum type.
public abstract class BaseEnemyFactory
{
    public abstract AIEnemy CreateEnemy(EEnemyType.EnemyType type, Vector3 position, Quaternion rotation);
}
