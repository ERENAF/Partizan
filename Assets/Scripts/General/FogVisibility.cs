
using UnityEngine;

[DefaultExecutionOrder(100)]
public class FogVisibility : MonoBehaviour
{
    [Header("Fog")]
    [SerializeField]
    private FogOfWar fogOfWar;

    [Header("Enemy")]
    [SerializeField]
    private Collider2D targetCollider;

    [SerializeField]
    private SpriteRenderer[] renderers;

    [Header("Settings")]
    [SerializeField, Range(0f, 1f)]
    private float visibilityThreshold = 0.5f;

    private void Awake()
    {
        if (fogOfWar == null)
            fogOfWar = FindFirstObjectByType<FogOfWar>();

        if (targetCollider == null)
            targetCollider = GetComponent<Collider2D>();

        if (renderers == null || renderers.Length == 0)
        {
            renderers =
                GetComponentsInChildren<SpriteRenderer>(true);
        }
    }

    private void LateUpdate()
    {
        if (fogOfWar == null)
            return;

        bool visible = CheckVisibility();

        // Применяем состояние каждый кадр.
        foreach (SpriteRenderer sr in renderers)
        {
            if (sr != null)
                sr.enabled = visible;
        }
    }

    private bool CheckVisibility()
    {
        if (targetCollider == null)
        {
            return fogOfWar.GetVisibility(
                transform.position
            ) >= visibilityThreshold;
        }

        Bounds bounds = targetCollider.bounds;

        Vector2 center = bounds.center;

        Vector2[] points =
        {
            center,
            new Vector2(bounds.min.x, center.y),
            new Vector2(bounds.max.x, center.y),
            new Vector2(center.x, bounds.min.y),
            new Vector2(center.x, bounds.max.y)
        };

        foreach (Vector2 point in points)
        {
            if (fogOfWar.GetVisibility(point)
                >= visibilityThreshold)
            {
                return true;
            }
        }

        return false;
    }

    private void OnDisable()
    {
        if (renderers == null)
            return;

        foreach (SpriteRenderer sr in renderers)
        {
            if (sr != null)
                sr.enabled = true;
        }
    }
}
