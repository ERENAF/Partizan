using System;
using UnityEngine;

[Serializable]
public class RangeAttack : Attack
{
    public override bool TryGetTarget(
        Transform origin,
        out Collider2D target
    )
    {
        return TryRaycast(
            origin,
            GetDirection(origin),
            out target
        );
    }

    public override void Execute(Transform origin)
    {
        if (!TryGetTarget(origin, out Collider2D target))
            return;

        Debug.Log($"Target in sight: {target.name}");
    }

    public void DrawGizmos(Transform origin)
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawRay(
            origin.position,
            GetDirection(origin) * attackDistance
        );
    }
}
