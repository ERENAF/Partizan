using System;
using UnityEngine;

public abstract class Attack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField,Min(0)] protected float attackDistance;
    [SerializeField,Min(0)] protected float damage = 25f;

    [Header("Layers")]
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    public abstract void Execute();
    public abstract bool TryGetTarget(out Collider2D target);
    protected bool TryRaycast(Vector2 direction, out Collider2D target)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction.normalized,
            attackDistance,
            targetMask.value | obstacleMask.value
        );

        target = hit.collider;

        return target != null && (targetMask.value & (1 << target.gameObject.layer)) != 0;
    }
}
