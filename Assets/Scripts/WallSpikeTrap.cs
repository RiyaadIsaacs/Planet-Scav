using System.Collections;
using UnityEngine;

public class WallSpikeTrap : MonoBehaviour
{
    [Header("Spike")]
    [SerializeField] private Transform _spike;
    [SerializeField] private Vector3 _extendedLocalOffset = new Vector3(0f, 0f, 2f);
    [SerializeField] private Collider _spikeCollider;

    [Header("Timing")]
    [SerializeField] private float _interval = 3f;
    [SerializeField] private float _extendedHoldTime = 0.75f;
    [SerializeField] private float _extendSpeed = 8f;
    [SerializeField] private float _retractSpeed = 6f;
    [SerializeField] private float _startDelay;

    [Header("Damage")]
    [SerializeField] private bool _killPlayerOnHit = true;
    [SerializeField] private float _bossDamage = 25f;

    private Vector3 _retractedLocalPosition;
    private Vector3 _extendedLocalPosition;
    private bool _isExtended;
    private Coroutine _trapRoutine;

    private void Awake()
    {
        if (_spike == null)
            _spike = transform;

        if (_spikeCollider == null)
            _spikeCollider = _spike.GetComponentInChildren<Collider>();

        if (_spikeCollider == null)
            _spikeCollider = GetComponentInChildren<Collider>();

        _retractedLocalPosition = _spike.localPosition;
        _extendedLocalPosition = _retractedLocalPosition + _extendedLocalOffset;

        SetupHitbox();
        SetSpikeColliderActive(false);
    }

    private void SetupHitbox()
    {
        if (_spikeCollider == null)
        {
            Debug.LogWarning($"{nameof(WallSpikeTrap)} on '{name}' has no spike collider assigned.", this);
            return;
        }

        _spikeCollider.isTrigger = true;

        var hitbox = _spikeCollider.GetComponent<WallSpikeTrapHitbox>();
        if (hitbox == null)
            hitbox = _spikeCollider.gameObject.AddComponent<WallSpikeTrapHitbox>();

        hitbox.Initialize(this);
    }

    private void OnEnable()
    {
        _trapRoutine = StartCoroutine(TrapLoop());
    }

    private void OnDisable()
    {
        if (_trapRoutine != null)
        {
            StopCoroutine(_trapRoutine);
            _trapRoutine = null;
        }

        _isExtended = false;
        _spike.localPosition = _retractedLocalPosition;
        SetSpikeColliderActive(false);
    }

    private IEnumerator TrapLoop()
    {
        if (_startDelay > 0f)
            yield return new WaitForSeconds(_startDelay);

        while (enabled)
        {
            yield return new WaitForSeconds(_interval);

            yield return MoveSpike(_extendedLocalPosition, _extendSpeed);
            _isExtended = true;
            SetSpikeColliderActive(true);

            yield return new WaitForSeconds(_extendedHoldTime);

            _isExtended = false;
            SetSpikeColliderActive(false);
            yield return MoveSpike(_retractedLocalPosition, _retractSpeed);
        }
    }

    private IEnumerator MoveSpike(Vector3 targetLocalPosition, float speed)
    {
        while (Vector3.Distance(_spike.localPosition, targetLocalPosition) > 0.01f)
        {
            _spike.localPosition = Vector3.MoveTowards(
                _spike.localPosition,
                targetLocalPosition,
                speed * Time.deltaTime);

            yield return null;
        }

        _spike.localPosition = targetLocalPosition;
    }

    private void SetSpikeColliderActive(bool isActive)
    {
        if (_spikeCollider == null)
            return;

        _spikeCollider.isTrigger = true;
        _spikeCollider.enabled = isActive;

        if (isActive)
            ProcessOverlappingTargets();
    }

    internal void HandleHit(Collider other)
    {
        if (!_isExtended || other == null)
            return;

        var player = other.GetComponentInParent<PlayerController>();
        if (_killPlayerOnHit && (player != null || other.CompareTag("Player")))
        {
            TryKillPlayer(player);
            return;
        }

        var boss = other.GetComponentInParent<BossEnemy>();
        if (boss != null && _bossDamage > 0f)
            boss.TakeDamage(_bossDamage);
    }

    private void ProcessOverlappingTargets()
    {
        if (_spikeCollider == null)
            return;

        var trapBounds = _spikeCollider.bounds;

        var player = FindFirstObjectByType<PlayerController>();
        if (player != null && PlayerIntersectsBounds(player, trapBounds))
            TryKillPlayer(player);

        var overlaps = Physics.OverlapBox(
            trapBounds.center,
            trapBounds.extents,
            _spikeCollider.transform.rotation,
            ~0,
            QueryTriggerInteraction.Collide);

        foreach (var overlap in overlaps)
        {
            if (overlap == _spikeCollider)
                continue;

            HandleHit(overlap);
        }
    }

    private void TryKillPlayer(PlayerController player)
    {
        var death = player?.deathHandling ?? FindFirstObjectByType<DeathHandling>();
        if (death != null)
            death.KillPlayer();
    }

    private static bool PlayerIntersectsBounds(PlayerController player, Bounds trapBounds)
    {
        if (player == null)
            return false;

        var controller = player.GetComponent<CharacterController>();
        if (controller == null)
            return trapBounds.Contains(player.transform.position);

        var center = controller.transform.TransformPoint(controller.center);
        var reach = Mathf.Max(controller.radius, controller.height * 0.5f);
        return trapBounds.SqrDistance(center) <= reach * reach;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var spikeTransform = _spike != null ? _spike : transform;
        var retracted = Application.isPlaying ? _retractedLocalPosition : spikeTransform.localPosition;
        var extended = retracted + _extendedLocalOffset;

        var retractedWorld = transform.TransformPoint(retracted);
        var extendedWorld = transform.TransformPoint(extended);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(retractedWorld, extendedWorld);
        Gizmos.DrawSphere(extendedWorld, 0.15f);
    }
#endif
}

internal class WallSpikeTrapHitbox : MonoBehaviour
{
    private WallSpikeTrap _trap;

    public void Initialize(WallSpikeTrap trap)
    {
        _trap = trap;
    }

    private void OnTriggerEnter(Collider other)
    {
        _trap?.HandleHit(other);
    }
}
