using System.Collections;
using UnityEngine;

/// <summary>
/// Drops assigned stalactite(s) after the player enters this trigger and a delay passes.
/// </summary>
[RequireComponent(typeof(Collider))]
public class StalactiteFallTrigger : MonoBehaviour
{
    [SerializeField] private FallingStalactite[] stalactites;
    [SerializeField] private float fallDelay = 2f;
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered;
    private Coroutine pendingFall;

    private void Awake()
    {
        var collider = GetComponent<Collider>();
        collider.isTrigger = true;

        if (stalactites == null || stalactites.Length == 0)
            stalactites = GetComponentsInParent<FallingStalactite>(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (triggerOnce && hasTriggered)
            return;

        if (pendingFall != null)
            return;

        hasTriggered = true;
        pendingFall = StartCoroutine(FallAfterDelay());
    }

    private IEnumerator FallAfterDelay()
    {
        if (fallDelay > 0f)
            yield return new WaitForSeconds(fallDelay);

        if (stalactites != null)
        {
            foreach (var stalactite in stalactites)
            {
                if (stalactite != null)
                    stalactite.TriggerFall();
            }
        }

        pendingFall = null;
    }

    private void OnDisable()
    {
        if (pendingFall != null)
        {
            StopCoroutine(pendingFall);
            pendingFall = null;
        }
    }
}
