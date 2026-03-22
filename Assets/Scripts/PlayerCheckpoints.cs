using System;
using System.Linq;
using UnityEngine;

public class PlayerCheckpoints : MonoBehaviour
{
    public Transform respawnPoints;

    [NonSerialized]
    public Vector3 _initialPosition;
    [NonSerialized]
    public Quaternion _initialRotation;

    private void Awake()
    {
        _initialPosition = respawnPoints.position;
        _initialRotation = respawnPoints.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CheckPoint"))
        {
            _initialPosition = other.transform.position;
            _initialRotation = other.transform.rotation;
        }
    }

}
