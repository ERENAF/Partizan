
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class RangeEnemy : EnemyAI
{
    [Header("Movement")]
    [SerializeField]
    private EnemyMovement movement;

    [SerializeField, Min(0f)]
    private float preferredDistance = 6f;

    [SerializeField, Min(0f)]
    private float retreatDistance = 2.5f;

    [SerializeField, Min(0f)]
    private float distanceTolerance = 0.5f;

    [Header("Attack")]
    [SerializeField]
    private RangeAttack rangeAttack = new();

    [SerializeField, Min(0f)]
    private float aimTime = 1.5f;

    [SerializeField, Min(0f)]
    private float attackCooldown = 2f;

    [SerializeField, Range(0f, 45f)]
    private float aimTolerance = 5f;

    [Header("Retreat")]
    [SerializeField, Min(0f)]
    private float retreatStep = 3f;

    private float aimProgress;
    private float attackTimer;

    public float AimProgress =>
        aimTime <= 0f
            ? 1f
            : Mathf.Clamp01(aimProgress / aimTime);

    protected override void Awake()
    {
        base.Awake();

        if (movement == null)
            movement = GetComponent<EnemyMovement>();
    }

    protected override void OnPlayerDetected()
    {
        currentState = State.Chase;
        aimProgress = 0f;
    }

    protected override void UpdateBehaviour(bool visible)
    {
        attackTimer = Mathf.Max(
            0f,
            attackTimer - Time.deltaTime
        );

        if (currentState == State.Patrol)
        {
            RotateTowardsMovement();

            aimProgress = 0f;
            return;
        }

        Vector2 targetPosition = visible
            ? (Vector2)player.position
            : lastKnownPosition;

        RotateTowards(targetPosition);

        if (!visible)
        {
            aimProgress = 0f;
            currentState = State.Chase;
            movement.MoveTo(lastKnownPosition);
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        // 1. Игрок слишком близко — отступаем.
        if (distance < retreatDistance)
        {
            aimProgress = 0f;
            currentState = State.Chase;

            RetreatFromPlayer();
            return;
        }

        // 2. Игрок далеко — приближаемся.
        if (distance > preferredDistance + distanceTolerance)
        {
            aimProgress = 0f;
            currentState = State.Chase;

            movement.MoveTo(player.position);
            return;
        }

        // 3. Игрок в подходящем диапазоне.
        movement.Stop();

        // Если луч заблокирован препятствием,
        // ищем позицию для стрельбы.
        if (!CanShootPlayer())
        {
            aimProgress = 0f;
            currentState = State.Chase;

            movement.MoveTo(player.position);
            return;
        }

        // 4. Прицеливаемся.
        currentState = State.Aim;

        if (attackTimer > 0f)
        {
            aimProgress = 0f;
            return;
        }

        if (!IsFacingPlayer())
        {
            aimProgress = 0f;
            return;
        }

        aimProgress += Time.deltaTime;

        if (aimProgress >= aimTime)
        {
            Shoot();
        }
    }

    private bool CanShootPlayer()
    {
        Transform origin = orientation != null
            ? orientation
            : transform;

        if (!rangeAttack.TryGetTarget(
            origin,
            out Collider2D target))
        {
            return false;
        }

        return IsPlayerCollider(target);
    }

    private bool IsPlayerCollider(Collider2D target)
    {
        if (target == null || player == null)
            return false;

        return target.transform == player ||
               target.transform.IsChildOf(player);
    }

    private bool IsFacingPlayer()
    {
        Transform origin = orientation != null
            ? orientation
            : transform;

        Vector2 direction =
            (Vector2)player.position -
            (Vector2)origin.position;

        if (direction.sqrMagnitude < 0.0001f)
            return false;

        Vector2 forward =
            Quaternion.Euler(
                0f,
                0f,
                -rotationOffset
            ) * origin.right;

        float angle = Vector2.Angle(
            forward,
            direction
        );

        return angle <= aimTolerance;
    }

    private void RetreatFromPlayer()
    {
        Vector2 away =
            (Vector2)transform.position -
            (Vector2)player.position;

        if (away.sqrMagnitude < 0.0001f)
        {
            away = Vector2.up;
        }

        away.Normalize();

        Vector2 retreatPosition =
            (Vector2)transform.position +
            away * retreatStep;

        movement.MoveTo(retreatPosition);
    }

    private void Shoot()
    {
        Transform origin = orientation != null
            ? orientation
            : transform;

        if (!rangeAttack.TryGetTarget(
            origin,
            out Collider2D target))
        {
            aimProgress = 0f;
            return;
        }

        if (!IsPlayerCollider(target))
        {
            aimProgress = 0f;
            return;
        }

        Death death =
            target.GetComponentInParent<Death>();

        aimProgress = 0f;
        attackTimer = attackCooldown;

        Debug.Log("RangeEnemy shoots!");

        if (death != null)
            death.death();
    }

    protected override void ForgetPlayer()
    {
        movement.Stop();

        aimProgress = 0f;

        base.ForgetPlayer();
    }

    private void OnDisable()
    {
        if (movement != null)
            movement.Stop();

        aimProgress = 0f;
        attackTimer = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (rangeAttack == null)
            return;

        Transform origin = orientation != null
            ? orientation
            : transform;

        rangeAttack.DrawGizmos(origin);

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            preferredDistance
        );

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            retreatDistance
        );
    }
}
