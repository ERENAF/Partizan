
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Seeker))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)]
    private float moveSpeed = 3f;

    [SerializeField, Min(0.01f)]
    private float waypointDistance = 0.3f;

    [SerializeField, Min(0f)]
    private float stoppingDistance = 0.8f;

    [Header("Pathfinding")]
    [SerializeField, Min(0.05f)]
    private float repathInterval = 0.3f;

    private Rigidbody2D rb;
    private Seeker seeker;

    private List<Vector3> waypoints;
    private int waypointIndex;

    private Vector2 destination;
    private float repathTimer;

    private bool isMoving;
    private int pathVersion;

    public bool IsMoving => isMoving;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();
    }

    public void MoveTo(Vector2 target)
    {
        destination = target;

        if (!isMoving)
        {
            isMoving = true;
            repathTimer = 0f;
        }
    }

    public void Stop()
    {
        isMoving = false;
        waypoints = null;
        waypointIndex = 0;

        // Игнорируем результаты старых запросов.
        pathVersion++;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void Update()
    {
        if (!isMoving)
            return;

        repathTimer -= Time.deltaTime;

        if (repathTimer <= 0f && seeker.IsDone())
        {
            repathTimer = repathInterval;

            int version = ++pathVersion;

            seeker.StartPath(
                transform.position,
                destination,
                path => OnPathComplete(path, version)
            );
        }
    }

    private void OnPathComplete(Path path, int version)
    {
        if (!isMoving || version != pathVersion)
            return;

        if (path.error)
        {
            waypoints = null;
            waypointIndex = 0;
            return;
        }

        waypoints = path.vectorPath;
        waypointIndex = 0;
    }

    private void FixedUpdate()
    {
        if (!isMoving)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (Vector2.Distance(rb.position, destination)
            <= stoppingDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (waypoints == null || waypoints.Count == 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        while (waypointIndex < waypoints.Count)
        {
            Vector2 waypoint = waypoints[waypointIndex];

            if (Vector2.Distance(rb.position, waypoint)
                > waypointDistance)
                break;

            waypointIndex++;
        }

        if (waypointIndex >= waypoints.Count)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 target = waypoints[waypointIndex];

        Vector2 direction = target - rb.position;

        rb.linearVelocity =
            direction.normalized * moveSpeed;
    }

    private void OnDisable()
    {
        Stop();
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints == null)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            Gizmos.DrawLine(
                waypoints[i],
                waypoints[i + 1]
            );
        }
    }
}
