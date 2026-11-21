
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class MeleeEnemy : EnemyAI
{
    [Header("Movement")]
    [SerializeField] private EnemyMovement movement;

    [Header("Attack")]
    [SerializeField] private MeleeAttack meleeAttack = new();

    [SerializeField, Min(0f)]
    private float attackCooldown = 1f;

    private float attackTimer;

    protected override void Awake()
    {
        base.Awake();

        if (movement == null)
            movement = GetComponent<EnemyMovement>();
    }

    protected override void OnPlayerDetected()
    {
        currentState = State.Chase;
    }

    protected override void UpdateBehaviour(bool visible)
    {
        attackTimer = Mathf.Max(
            0f,
            attackTimer - Time.deltaTime
        );

        if (currentState != State.Chase)
        {
            movement.Stop();
            return;
        }

        Vector2 targetPosition = visible
            ? (Vector2)player.position
            : lastKnownPosition;

        // Продолжаем смотреть на игрока
        // или на его последнюю известную позицию.
        RotateTowards(targetPosition);

        // Сначала проверяем возможность атаки.
        if (visible && attackTimer <= 0f)
        {
            Transform origin = orientation != null
                ? orientation
                : transform;

            if (meleeAttack.TryGetTarget(
                origin,
                out Collider2D target))
            {
                Death death = target.GetComponentInParent<Death>();

                if (death != null &&
                    (death.transform == player ||
                     death.transform.IsChildOf(player)))
                {
                    movement.Stop();

                    attackTimer = attackCooldown;
                    death.death();
                    return;
                }
            }
        }

        // Во время перезарядки или если атака
        // не достаёт до игрока — продолжаем путь.
        movement.MoveTo(targetPosition);
    }

    protected override void ForgetPlayer()
    {
        base.ForgetPlayer();
        movement.Stop();
    }

    private void OnDisable()
    {
        if (movement != null)
            movement.Stop();
    }

    private void OnDrawGizmosSelected()
    {
        if (meleeAttack == null)
            return;

        Transform origin = orientation != null
            ? orientation
            : transform;

        meleeAttack.DrawGizmos(origin);
    }
}
