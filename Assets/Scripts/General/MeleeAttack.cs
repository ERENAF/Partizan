using System;
using UnityEngine;

public class MeleeAttack : Attack
{
    [Header("Melee")]
    [SerializeField,Range(-180,180)]
    private float attackAngle = 90f;

    [SerializeField,Min(2)] private int rayCount = 15;

    public override bool TryGetTarget(out Collider2D target)
    {
        target = null;

        float step = attackAngle / (rayCount - 1);
        float closestDistance = float.PositiveInfinity;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = -attackAngle / 2f + step * i;

            Vector2 direction =
                Quaternion.Euler(0f, 0f, angle)
                * transform.right;

            if (!TryRaycast(direction, out Collider2D hit))
                continue;

            float distance = (
                hit.ClosestPoint(transform.position) -
                (Vector2)transform.position
            ).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                target = hit;
            }
        }

        return target != null;
    }

    public override void Execute()
    {
        if (!TryGetTarget(out Collider2D target))
            return;

        Debug.Log($"Melee hit: {target.name}");

        target.GetComponent<Death>().death();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        int count = Mathf.Max(2, rayCount);
        float step = attackAngle / (count - 1);

        for (int i = 0; i < count; i++)
        {
            float angle = -attackAngle / 2f + step * i;

            Vector3 direction =
                Quaternion.Euler(0f, 0f, angle)
                * transform.right;

            Gizmos.DrawRay(
                transform.position,
                direction * attackDistance
            );
        }
    }
}
