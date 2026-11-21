
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FogOfWar : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform orientation;

    [Header("Cone Vision")]
    [SerializeField, Min(0.1f)]
    private float viewDistance = 8f;

    [SerializeField, Range(1f, 360f)]
    private float viewAngle = 100f;

    [SerializeField]
    private float angleOffset = 90f;

    [Header("Near Vision")]
    [SerializeField, Min(0f)]
    private float nearViewRadius = 1.5f;

    [Header("Obstacles")]
    [SerializeField]
    private LayerMask obstacleMask;

    [Header("Quality")]
    [SerializeField, Range(128, 2048)]
    private int textureResolution = 512;

    [SerializeField, Range(90, 1440)]
    private int rayCount = 720;

    [Header("Fog")]
    [SerializeField, Range(0f, 1f)]
    private float fogOpacity = 1f;

    [SerializeField]
    private Vector2 worldSize = new(40f, 40f);

    private Texture2D fogTexture;
    private Sprite generatedSprite;
    private SpriteRenderer fogRenderer;

    private Color32[] pixels;
    private float[] rayDistances;

    private float facingAngle;
    private bool visionReady;

    private void Awake()
    {
        fogRenderer = GetComponent<SpriteRenderer>();
        CreateTexture();
    }

    private void LateUpdate()
    {
        if (player == null || orientation == null)
        {
            visionReady = false;
            return;
        }

        UpdateRays();
        UpdateTexture();
    }

    private void CreateTexture()
    {
        fogTexture = new Texture2D(
            textureResolution,
            textureResolution,
            TextureFormat.RGBA32,
            false
        );

        fogTexture.filterMode = FilterMode.Bilinear;
        fogTexture.wrapMode = TextureWrapMode.Clamp;

        pixels = new Color32[
            textureResolution * textureResolution
        ];

        rayDistances = new float[rayCount];

        // В начале вся карта скрыта.
        byte alpha = (byte)(fogOpacity * 255f);

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color32(
                0, 0, 0, alpha
            );
        }

        fogTexture.SetPixels32(pixels);
        fogTexture.Apply(false);

        generatedSprite = Sprite.Create(
            fogTexture,
            new Rect(
                0,
                0,
                textureResolution,
                textureResolution
            ),
            new Vector2(0.5f, 0.5f),
            textureResolution / worldSize.x
        );

        fogRenderer.sprite = generatedSprite;
        fogRenderer.sortingOrder = 1000;

        // FogOfWar должен быть корневым объектом
        // с исходным масштабом (1, 1, 1).
        transform.localScale = new Vector3(
            1f,
            worldSize.y / worldSize.x,
            1f
        );
    }

    private void UpdateRays()
    {
        Vector2 origin = player.position;

        facingAngle =
            orientation.eulerAngles.z + angleOffset;

        float maxDistance = Mathf.Max(
            viewDistance,
            nearViewRadius
        );

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * 360f / rayCount;
            float radians = angle * Mathf.Deg2Rad;

            Vector2 direction = new Vector2(
                Mathf.Cos(radians),
                Mathf.Sin(radians)
            );

            RaycastHit2D hit = Physics2D.Raycast(
                origin,
                direction,
                maxDistance,
                obstacleMask
            );

            rayDistances[i] = hit.collider != null
                ? hit.distance
                : maxDistance;
        }

        visionReady = true;
    }

    // Расстояние до стены по ближайшему лучу.
    private float GetWallDistance(float angle)
    {
        float normalized =
            Mathf.Repeat(angle, 360f);

        int index = Mathf.RoundToInt(
            normalized / 360f * rayCount
        ) % rayCount;

        return rayDistances[index];
    }

    // Видимость точки от 0 до 1.
    private float CalculateVisibility(Vector2 point)
    {
        Vector2 direction =
            point - (Vector2)player.position;

        float distance = direction.magnitude;

        if (distance < 0.001f)
            return 1f;

        float maxDistance = Mathf.Max(
            viewDistance,
            nearViewRadius
        );

        if (distance > maxDistance)
            return 0f;

        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        // Стены перекрывают обе области.
        float wallDistance = GetWallDistance(angle);

        if (distance > wallDistance)
            return 0f;

        // 1. Круговой обзор на 360 градусов.
        bool insideCircle =
            distance <= nearViewRadius;

        // 2. Направленный конус зрения.
        float delta = Mathf.Abs(
            Mathf.DeltaAngle(
                facingAngle,
                angle
            )
        );

        bool insideCone =
            delta <= viewAngle * 0.5f &&
            distance <= viewDistance;

        // Точка видна, если попадает
        // хотя бы в одну область.
        return insideCircle || insideCone
            ? 1f
            : 0f;
    }

    // Для FogVisibility и других компонентов.
    public float GetVisibility(Vector2 worldPoint)
    {
        if (!visionReady || player == null)
            return 0f;

        return CalculateVisibility(worldPoint);
    }

    private void UpdateTexture()
    {
        int size = textureResolution;

        Vector2 bottomLeft =
            (Vector2)transform.position -
            worldSize * 0.5f;

        byte fogAlpha =
            (byte)(fogOpacity * 255f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 worldPoint = new Vector2(
                    bottomLeft.x +
                    (x + 0.5f) / size * worldSize.x,

                    bottomLeft.y +
                    (y + 0.5f) / size * worldSize.y
                );

                float visibility =
                    CalculateVisibility(worldPoint);

                pixels[y * size + x] =
                    new Color32(
                        0,
                        0,
                        0,
                        visibility > 0f
                            ? (byte)0
                            : fogAlpha
                    );
            }
        }

        fogTexture.SetPixels32(pixels);
        fogTexture.Apply(false);
    }

    private void OnDestroy()
    {
        if (fogRenderer != null)
            fogRenderer.sprite = null;

        if (generatedSprite != null)
            Destroy(generatedSprite);

        if (fogTexture != null)
            Destroy(fogTexture);
    }
}
