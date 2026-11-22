
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Points")]
    [SerializeField]
    private Transform[] patrolPoints;

    [Header("Settings")]
    [SerializeField, Min(0f)]
    private float waitTime = 1.5f;

    [SerializeField, Min(0.01f)]
    private float stoppingDistance = 0.15f;

    [SerializeField]
    private bool loop = true;

    private EnemyMovement movement;

    private int currentPoint;
    private float waitTimer;

    private bool isWaiting;
    private bool isPatrolling;

    public bool IsPatrolling => isPatrolling;

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
    }

    public void StartPatrol()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
            return;

        if (isPatrolling)
            return;

        isPatrolling = true;
        isWaiting = false;

        MoveToCurrentPoint();
    }

    public void StopPatrol()
    {
        if (!isPatrolling)
            return;

        isPatrolling = false;
        isWaiting = false;

        movement.Stop();
    }

    private void Update()
    {
        if (!isPatrolling)
            return;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                NextPoint();
            }

            return;
        }

        if (movement.HasReachedDestination)
        {
            movement.Stop();

            isWaiting = true;
            waitTimer = waitTime;
        }
    }

    private void NextPoint()
    {
        if (loop)
        {
            currentPoint =
                (currentPoint + 1) % patrolPoints.Length;
        }
        else
        {
            if (currentPoint >= patrolPoints.Length - 1)
            {
                isPatrolling = false;
                return;
            }

            currentPoint++;
        }

        MoveToCurrentPoint();
    }

    private void MoveToCurrentPoint()
    {
        Transform point = patrolPoints[currentPoint];

        if (point == null)
        {
            Debug.LogWarning(
                $"Patrol point {currentPoint} is null."
            );

            StopPatrol();
            return;
        }

        movement.MoveTo(
            point.position,
            stoppingDistance
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (patrolPoints == null)
            return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null)
                continue;

            Gizmos.DrawWireSphere(
                patrolPoints[i].position,
                0.2f
            );

            int next = i + 1;

            if (next >= patrolPoints.Length)
            {
                if (!loop)
                    continue;

                next = 0;
            }

            if (patrolPoints[next] != null)
            {
                Gizmos.DrawLine(
                    patrolPoints[i].position,
                    patrolPoints[next].position
                );
            }
        }
    }
}
