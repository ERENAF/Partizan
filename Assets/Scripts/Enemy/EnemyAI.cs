
using UnityEngine;

public abstract class EnemyAI : Rotation
{
    public enum State
    {
        Patrol,
        Suspicious,
        Chase,
        Aim
    }

    [Header("References")]
    [SerializeField] protected Transform player;
    [SerializeField] protected EnemyVision vision;
    [SerializeField] protected Transform orientation;
    [SerializeField] protected EnemyPatrol patrol;
    [SerializeField] protected EnemyMovement movement;
    [Header("Detection")]
    [SerializeField, Min(0f)]
    protected float detectionTime = 1.5f;

    [SerializeField, Min(0f)]
    protected float forgetTime = 2f;

    [Header("Rotation")]
    [SerializeField, Min(0f)]
    protected float rotationSpeed = 360f;

    [SerializeField]
    protected float rotationOffset = 0f;

    protected State currentState = State.Patrol;

    protected float detectionProgress;
    protected float lostTargetTimer;

    protected Vector2 lastKnownPosition;

    protected virtual void Awake()
    {
        if (orientation == null)
            orientation = transform;

        if (vision == null)
            vision = GetComponent<EnemyVision>();

        if (patrol == null)
            patrol = GetComponent<EnemyPatrol>();
        if (movement == null)
            movement = GetComponent<EnemyMovement>();
    }
    protected void RotateTowardsMovement()
    {
        if (movement == null)
            return;

        Vector2 direction = movement.MoveDirection;

        if (direction.sqrMagnitude < 0.001f)
            return;

        RotateTowards(
            (Vector2)orientation.position + direction
        );
    }

    protected virtual void Start()
    {
        patrol?.StartPatrol();
    }

    protected virtual void Update()
    {
        if (player == null || vision == null)
            return;

        bool visible = vision.CanSee(player);

        UpdateDetection(visible);
        UpdateBehaviour(visible);
    }

    private void UpdateDetection(bool visible)
    {
        if (visible)
        {
            lostTargetTimer = 0f;
            lastKnownPosition = player.position;

            if (currentState == State.Patrol ||
                currentState == State.Suspicious)
            {
                if (currentState == State.Patrol)
                {
                    currentState = State.Suspicious;
                    patrol?.StopPatrol();
                }

                detectionProgress += Time.deltaTime;

                if (detectionProgress >= detectionTime)
                {
                    detectionProgress = detectionTime;

                    OnPlayerDetected();
                }
            }
        }
        else
        {
            detectionProgress = 0f;

            if (currentState == State.Suspicious)
            {
                currentState = State.Patrol;
                patrol?.StartPatrol();
            }

            if (currentState == State.Chase ||
                currentState == State.Aim)
            {
                lostTargetTimer += Time.deltaTime;

                if (lostTargetTimer >= forgetTime)
                {
                    ForgetPlayer();
                }
            }
        }
    }

    protected virtual void ForgetPlayer()
    {
        currentState = State.Patrol;

        detectionProgress = 0f;
        lostTargetTimer = 0f;

        patrol?.StartPatrol();
    }

    protected void RotateTowards(Vector2 targetPosition)
    {
        Transform rotationTransform = orientation != null
            ? orientation
            : transform;

        Vector2 currentPosition = rotationTransform.position;
        Vector2 direction = targetPosition - currentPosition;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        float angle = Rotate(currentPosition, targetPosition);

        Quaternion targetRotation = Quaternion.Euler(
            0f,
            0f,
            angle + rotationOffset
        );

        rotationTransform.rotation = Quaternion.RotateTowards(
            rotationTransform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    protected abstract void OnPlayerDetected();

    protected abstract void UpdateBehaviour(bool visible);
}
