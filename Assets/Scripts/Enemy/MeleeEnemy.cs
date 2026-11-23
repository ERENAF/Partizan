
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class MeleeEnemy : EnemyAI
{

    [Header("Attack")]
    [SerializeField]
    private MeleeAttack meleeAttack = new();

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

        // 1. Патрулирование
        if (currentState == State.Patrol)
        {
            RotateTowardsMovement();
            return;
        }

        // 2. Подозрение
        if (currentState == State.Suspicious)
        {
            if (visible)
                RotateTowards(player.position);

            return;
        }

        // 3. Преследование
        if (currentState != State.Chase)
            return;

        Vector2 targetPosition = visible
            ? (Vector2)player.position
            : lastKnownPosition;

        RotateTowards(targetPosition);

        // 4. Проверяем возможность атаки
        if (visible && attackTimer <= 0f)
        {
            Transform origin = orientation != null
                ? orientation
                : transform;

            if (meleeAttack.TryGetTarget(
                origin,
                out Collider2D target))
            {
                if (target != null)
                {
                    Death death =
                        target.GetComponentInParent<Death>();

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
        }

        // 5. Продолжаем движение через A*
        movement.MoveTo(targetPosition);
    }

    protected override void ForgetPlayer()
    {
        // Сначала останавливаем преследование
        movement.Stop();

        // Затем запускаем патруль
        base.ForgetPlayer();
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
