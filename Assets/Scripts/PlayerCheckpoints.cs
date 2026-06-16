using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCheckpoints : MonoBehaviour
{
    public Transform respawnPoints;

    [NonSerialized]
    public Vector3 initialPosition;
    [NonSerialized]
    public Quaternion initialRotation;

    // Internal stack of respawn point transforms
    private Stack<Transform> checkPointStack = new Stack<Transform>();

    private void Awake()
    {
        if (respawnPoints != null)
            ApplySpawnPoint(respawnPoints);
    }

    public void SetSpawnPoint(Transform spawn)
    {
        respawnPoints = spawn;
        ApplySpawnPoint(spawn);
    }

    private void ApplySpawnPoint(Transform spawn)
    {
        if (spawn == null)
            return;

        initialPosition = spawn.position;
        initialRotation = spawn.rotation;
        checkPointStack.Clear();
        checkPointStack.Push(spawn);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("CheckPoint"))
            return;

        if (CompareTag("Player"))
        {


            Transform newCheckPoint = other.transform;

            // Keep the public field in sync
            respawnPoints = newCheckPoint;

            // If stack is empty, add the new respawn point
            if (checkPointStack.Count == 0)
            {
                checkPointStack.Push(newCheckPoint);
                UpdateInitialFromTop();
                return;
            }

            // If stack has exactly one item, do nothing (per your rule)
            if (checkPointStack.Count == 1)
            {
                checkPointStack.Pop();
                checkPointStack.Push(newCheckPoint);
                UpdateInitialFromTop();
            }

            // If stack has more than one item: remove the current top and add the new one
            //if (checkPointStack.Count > 1)
            //{

            //}

        }

        else
        {
                       return;
        }

    }

    private void UpdateInitialFromTop()
    {
        if (checkPointStack.Count == 0)
            return;

        Transform top = checkPointStack.Peek();
        if (top == null)
            return;

        initialPosition = top.position;
        initialRotation = top.rotation;
    }

    // Optional helpers you can call from other code
    public Transform PeekRespawn() => checkPointStack.Count > 0 ? checkPointStack.Peek() : null;
    public int RespawnStackCount() => checkPointStack.Count;
}
