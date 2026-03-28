using Fusion;
using UnityEngine;

public class PlayerAttack : NetworkBehaviour
{
    [Header("Attack Settings")]
    public float punchDamage = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;

    [Header("References")]
    public Transform attackOrigin; // Khu vực đấm ra (tùy chọn)
    public LayerMask enemyLayer;   // Layer của đối thủ

    [Networked] private TickTimer attackCooldownTimer { get; set; }

    [Networked, OnChangedRender(nameof(PlayPunchAnimation))]
    public NetworkBool isPunching { get; set; }

    public Animator _animator;

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput<NetworkInputData>(out var input))
        {
            if (input.isAttackPressed)
            {
                if (attackCooldownTimer.ExpiredOrNotRunning(Runner))
                {
                    ExecutePunch();
                    attackCooldownTimer = TickTimer.CreateFromSeconds(Runner, attackCooldown);
                }
            }
        }
    }

    private void ExecutePunch()
    {
        isPunching = !isPunching; // Triggers animation qua mạng
        
        if (HasInputAuthority || HasStateAuthority) 
            PlayPunchAnimationLocally();

        Vector3 originPos = attackOrigin != null ? attackOrigin.position : transform.position + Vector3.up * 1f;
        Vector3 direction = attackOrigin != null ? attackOrigin.forward : transform.forward;

        RaycastHit hit;
        if (Physics.SphereCast(originPos, 0.5f, direction, out hit, attackRange, enemyLayer))
        {
            if (hit.collider.TryGetComponent(out HealthBase targetHealth))
            {
                if (hit.collider.TryGetComponent(out NetworkObject hitNetworkObj))
                {
                    if (HasStateAuthority)
                        targetHealth.TakeDamage(punchDamage);
                    else
                        Rpc_DealDamage(hitNetworkObj, punchDamage);
                }
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void Rpc_DealDamage(NetworkObject targetObj, float damage)
    {
        if (targetObj.TryGetComponent(out HealthBase health))
        {
            health.TakeDamage(damage);
        }
    }

    private void PlayPunchAnimation()
    {
        if (!HasInputAuthority) PlayPunchAnimationLocally();
    }

    private void PlayPunchAnimationLocally()
    {
        if (_animator != null) _animator.SetTrigger("Attack"); 
    }
}
