
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraDeadZone : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;

    [Header("Dead Zone")]
    [SerializeField, Min(0.1f)]
    private float width = 4f;

    [SerializeField, Min(0.1f)]
    private float height = 3f;

    [Header("Mouse Follow")]
    [SerializeField] private bool enableMouseFollow = true;

    [SerializeField, Min(0f)]
    private float maxLookAhead = 2f;

    [SerializeField, Range(0f, 1f)]
    private float mouseInfluence = 0.5f;

    [SerializeField, Min(0.01f)]
    private float mouseSmoothTime = 0.25f;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;

    private Camera cam;

    private Vector2 basePosition;
    private Vector2 mouseOffset;
    private Vector2 mouseVelocity;

    private float cameraZ;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cameraZ = transform.position.z;
    }

    private void Start()
    {
        if (player == null)
            return;

        basePosition = player.position;

        transform.position = new Vector3(
            basePosition.x,
            basePosition.y,
            cameraZ
        );
    }


    private void LateUpdate()
    {
        if (player == null)
            return;

        Vector2 playerPos = player.position;

        float halfW = width * 0.5f;
        float halfH = height * 0.5f;

        // 1. DeadZone: обновляем базовый центр.
        // Это та же логика, которая у нас уже работала.
        basePosition.x = Mathf.Clamp(
            basePosition.x,
            playerPos.x - halfW,
            playerPos.x + halfW
        );

        basePosition.y = Mathf.Clamp(
            basePosition.y,
            playerPos.y - halfH,
            playerPos.y + halfH
        );

        // 2. MouseFollow.
        Vector2 targetOffset = Vector2.zero;

        if (enableMouseFollow && maxLookAhead > 0f)
        {
            Rect rect = cam.pixelRect;

            Vector2 screenDelta =
                (Vector2)Mouse.current.position.ReadValue() - rect.center;

            float unitsPerPixel =
                cam.orthographicSize * 2f / rect.height;

            Vector2 worldDelta =
                screenDelta * unitsPerPixel;

            targetOffset = Vector2.ClampMagnitude(
                worldDelta * mouseInfluence,
                maxLookAhead
            );
        }

        // 3. Сглаживаем только смещение курсора.
        mouseOffset = Vector2.SmoothDamp(
            mouseOffset,
            targetOffset,
            ref mouseVelocity,
            mouseSmoothTime
        );

        // 4. Итоговая позиция камеры.
        // Никаких дополнительных Clamp!
        Vector2 finalPosition = basePosition + mouseOffset;

        transform.position = new Vector3(
            finalPosition.x,
            finalPosition.y,
            cameraZ
        );
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        // Логическая DeadZone.
        Gizmos.color = Color.cyan;

        Vector3 baseCenter = Application.isPlaying
            ? new Vector3(
                basePosition.x,
                basePosition.y,
                transform.position.z
            )
            : transform.position;

        Gizmos.DrawWireCube(
            baseCenter,
            new Vector3(width, height, 0f)
        );

        // Реальная DeadZone камеры.
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(width, height, 0f)
        );
    }
}
