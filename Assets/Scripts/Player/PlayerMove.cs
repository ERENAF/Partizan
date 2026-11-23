
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMove : MonoBehaviour, IPausable
{
    [Header("Speed")]
    [SerializeField] private Speed speed = new();
    [SerializeField, Min(0)] private float moveSpeed;
    [SerializeField, Min(0)] private float sneakSpeed;
    [SerializeField, Min(0)] private float sprintSpeed;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference sneakAction;
    [SerializeField] private InputActionReference sprintAction;

    [Header("Footsteps")]
    [SerializeField] private AudioClip[] footstepClips;

    [SerializeField, Min(0.05f)]
    private float walkStepInterval = 0.45f;

    [SerializeField, Min(0.05f)]
    private float sprintStepInterval = 0.28f;

    [SerializeField, Min(0.05f)]
    private float sneakStepInterval = 0.7f;

    [SerializeField, Range(0f, 1f)]
    private float walkVolume = 0.65f;

    [SerializeField, Range(0f, 1f)]
    private float sprintVolume = 0.9f;

    [SerializeField, Range(0f, 1f)]
    private float sneakVolume = 0.2f;

    private Rigidbody2D rb;
    private AudioSource audioSource;

    private Vector3 direction;
    private float stepTimer;
    private int lastFootstepIndex = -1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        sprintAction.action.Enable();
        sneakAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        sprintAction.action.Disable();
        sneakAction.action.Disable();

        speed.Reset();
        stepTimer = 0f;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private float SpeedMultiplier()
    {
        if (sneakAction.action.IsPressed())
            return sneakSpeed;

        if (sprintAction.action.IsPressed())
            return sprintSpeed;

        return 1f;
    }

    private void Update()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();
        input = Vector2.ClampMagnitude(input, 1f);

        if (input.sqrMagnitude > 0.0001f)
        {
            direction = new Vector3(
                input.x,
                input.y,
                0f
            ).normalized;
        }

        float targetSpeed =
            moveSpeed * SpeedMultiplier() * input.magnitude;

        float currentSpeed =
            speed.Update(targetSpeed, Time.deltaTime);

        rb.linearVelocity = direction * currentSpeed;

        UpdateFootsteps();
    }

    private void UpdateFootsteps()
    {
        if (footstepClips == null ||
            footstepClips.Length == 0)
            return;

        float actualSpeed = rb.linearVelocity.magnitude;

        if (actualSpeed < 0.1f)
        {
            stepTimer = 0f;
            return;
        }

        bool sneaking = sneakAction.action.IsPressed();
        bool sprinting =
            !sneaking && sprintAction.action.IsPressed();

        float interval = walkStepInterval;
        float volume = walkVolume;

        if (sneaking)
        {
            interval = sneakStepInterval;
            volume = sneakVolume;
        }
        else if (sprinting)
        {
            interval = sprintStepInterval;
            volume = sprintVolume;
        }

        stepTimer -= Time.deltaTime;

        if (stepTimer > 0f)
            return;

        PlayFootstep(volume);

        // Частота шагов зависит от реальной скорости.
        float referenceSpeed = Mathf.Max(
            0.01f,
            moveSpeed * SpeedMultiplier()
        );

        float speedRatio = Mathf.Clamp(
            actualSpeed / referenceSpeed,
            0.25f,
            2f
        );

        stepTimer = interval / speedRatio;
    }

    private void PlayFootstep(float volume)
    {
        int index = Random.Range(
            0,
            footstepClips.Length
        );

        // Избегаем повторения предыдущего звука.
        if (footstepClips.Length > 1 &&
            index == lastFootstepIndex)
        {
            index = (index + 1) % footstepClips.Length;
        }

        lastFootstepIndex = index;

        AudioClip clip = footstepClips[index];

        if (clip == null)
            return;

        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.PlayOneShot(clip, volume);
    }
}
