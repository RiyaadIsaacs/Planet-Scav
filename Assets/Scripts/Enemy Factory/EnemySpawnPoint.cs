using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    public EEnemyType.EnemyType type = EEnemyType.EnemyType.Patrol;
    [Tooltip("Optional override. If null, spawn at this transform's position and rotation.")]
    public Transform spawnTransform;
    [Header("Optional (Patrol)")]
    [Tooltip("If set, EnemyMovement on the spawned enemy will be assigned this path.")]
    public PatrolWaypointPath patrolPath;
    public Vector3 SpawnPosition => (spawnTransform != null ? spawnTransform.position : transform.position);
    public Quaternion SpawnRotation => (spawnTransform != null ? spawnTransform.rotation : transform.rotation);
}

