using System;
using UnityEngine;

[Serializable]
public class MeleeAttack : Attack
{
    [Header("Melee")]
    [SerializeField, Range(0f, 360f)]
    private float attackAngle = 90f;

    [SerializeField, Min(2)]
    private int rayCount = 15;

    public override bool TryGetTarget(
        Transform origin,
        out Collider2D target
    )
    {
        target = null;

        int count = Mathf.Max(2, rayCount);
        float step = attackAngle / (count - 1);

        float closestDistance = float.PositiveInfinity;

        for (int i = 0; i < count; i++)
        {
            float angle = -attackAngle / 2f + step * i;

            Vector2 direction = GetDirection(origin, angle);

            if (!TryRaycast(origin, direction, out Collider2D hit))
                continue;

            float distance = (
                (Vector2)hit.ClosestPoint(origin.position) -
                (Vector2)origin.position
            ).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                target = hit;
            }
        }

        return target != null;
    }

    public override void Execute(Transform origin)
    {
        if (!TryGetTarget(origin, out Collider2D target))
            return;

        Debug.Log($"Melee hit: {target.name}");

        if (target.GetComponent<Death>() != null) target.GetComponent<Death>().death();
    }

    public void DrawGizmos(Transform origin)
    {
        Gizmos.color = Color.red;

        int count = Mathf.Max(2, rayCount);
        float step = attackAngle / (count - 1);

        for (int i = 0; i < count; i++)
        {
            float angle = -attackAngle / 2f + step * i;

            Vector2 direction = GetDirection(origin, angle);

            Gizmos.DrawRay(
                origin.position,
                direction * attackDistance
            );
        }
    }
}
