using UnityEngine;

public class EnemyVision : MonoBehaviour, IPausable
{
    [Header("Vision")]
    [SerializeField, Min(0f)] private float viewDistance = 8f;
    [SerializeField, Range(0f, 360f)] private float viewAngle = 100f;
    [SerializeField] private float angleOffset;

    [Header("Layers")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("References")]
    [SerializeField] private Transform visionOrigin;

    private Transform Origin =>
        visionOrigin != null ? visionOrigin : transform;

    public bool CanSee(Transform target)
    {
        if (target == null)
            return false;

        Vector2 origin = Origin.position;
        Vector2 direction = (Vector2)target.position - origin;

        float distance = direction.magnitude;

        if (distance > viewDistance)
            return false;

        if (distance < 0.001f)
            return true;

        Vector2 forward =
            Quaternion.Euler(0f, 0f, angleOffset) * Origin.right;

        if (Vector2.Angle(forward, direction) > viewAngle / 2f)
            return false;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            direction.normalized,
            distance,
            obstacleMask
        );

        return hit.collider == null;
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = Origin;

        Vector3 forward =
            Quaternion.Euler(0f, 0f, angleOffset) * origin.right;

        float halfAngle = viewAngle / 2f;

        Gizmos.color = Color.yellow;

        for (int i = 0; i <= 30; i++)
        {
            float angle = -halfAngle + viewAngle * i / 30f;

            Vector3 direction =
                Quaternion.Euler(0f, 0f, angle) * forward;

            Gizmos.DrawRay(
                origin.position,
                direction * viewDistance
            );
        }
    }
}
