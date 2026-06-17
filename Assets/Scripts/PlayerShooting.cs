using System.Collections;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireCooldown = 0.4f;
    [SerializeField] private float normalBossDamage = 25f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string shootTriggerName = "Shoot";
    [SerializeField] private string shootStateName = "Shoot";
    [SerializeField] private int upperBodyLayerIndex = 1;

    private Transform cameraPivot;
    private float nextFireTime;
    private bool shootingEnabled;
    private bool hasBossKillerShot;
    private int shootTriggerHash;
    private int shootStateHash;
    private Coroutine upperBodyLayerCoroutine;

    public bool HasBossKillerShot => hasBossKillerShot;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        shootTriggerHash = Animator.StringToHash(shootTriggerName);
        shootStateHash = Animator.StringToHash(shootStateName);
        EnsureUpperBodyLayer();
    }

    public void Configure(bool enabled, Transform pivot)
    {
        shootingEnabled = enabled;
        cameraPivot = pivot;

        if (firePoint == null && pivot != null)
            firePoint = pivot;

        EnsureUpperBodyLayer();
    }

    private void EnsureUpperBodyLayer()
    {
        if (animator == null)
            return;

        animator.SetLayerWeight(0, 1f);
        if (upperBodyLayerIndex > 0 && upperBodyLayerIndex < animator.layerCount)
            animator.SetLayerWeight(upperBodyLayerIndex, 0f);
    }

    public void GrantBossKillerShot()
    {
        hasBossKillerShot = true;
    }

    public bool TryShoot()
    {
        if (!shootingEnabled || projectilePrefab == null)
            return false;

        if (Time.time < nextFireTime)
            return false;

        if (Time.timeScale <= 0f)
            return false;

        var dialogueUI = FindFirstObjectByType<DialogueUIManager>();
        if (dialogueUI != null && dialogueUI.IsDialoguePanelVisible)
            return false;

        var aimTransform = firePoint != null ? firePoint : cameraPivot;
        if (aimTransform == null)
            return false;

        PlayShootAnimation();
        SFXManager.Play(SFXManager.ShootSoundId);

        var spawnPos = aimTransform.position + aimTransform.forward * 0.5f;
        var direction = aimTransform.forward;
        var projectileGo = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(direction));

        if (projectileGo.TryGetComponent<PlayerProjectile>(out var projectile))
        {
            projectile.Initialize(direction, normalBossDamage, hasBossKillerShot);
        }

        nextFireTime = Time.time + fireCooldown;
        return true;
    }

    private void PlayShootAnimation()
    {
        if (animator == null)
            return;

        if (upperBodyLayerIndex > 0 && upperBodyLayerIndex < animator.layerCount)
            animator.SetLayerWeight(upperBodyLayerIndex, 1f);

        animator.ResetTrigger(shootTriggerHash);
        animator.SetTrigger(shootTriggerHash);

        if (upperBodyLayerCoroutine != null)
            StopCoroutine(upperBodyLayerCoroutine);

        upperBodyLayerCoroutine = StartCoroutine(ReleaseUpperBodyLayerAfterShoot());
    }

    private IEnumerator ReleaseUpperBodyLayerAfterShoot()
    {
        yield return null;

        var duration = 0.35f;
        if (animator != null)
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(upperBodyLayerIndex);
            if (stateInfo.shortNameHash == shootStateHash)
                duration = stateInfo.length;
        }

        yield return new WaitForSeconds(duration);

        if (animator != null && upperBodyLayerIndex > 0 && upperBodyLayerIndex < animator.layerCount)
            animator.SetLayerWeight(upperBodyLayerIndex, 0f);

        upperBodyLayerCoroutine = null;
    }

    private void OnDisable()
    {
        if (upperBodyLayerCoroutine != null)
        {
            StopCoroutine(upperBodyLayerCoroutine);
            upperBodyLayerCoroutine = null;
        }

        EnsureUpperBodyLayer();
    }
}
