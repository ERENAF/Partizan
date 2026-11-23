
using System;
using UnityEngine;

[Serializable]
public abstract class Attack
{
    [Header("Attack")]
    [SerializeField, Min(0f)]
    protected float attackDistance = 1.5f;

    [SerializeField]
    protected float angleOffset;

    [Header("Layers")]
    [SerializeField]
    protected LayerMask targetMask;

    [SerializeField]
    protected LayerMask obstacleMask;

    [Header("Audio")]
    [SerializeField]
    private AudioClip[] attackSounds;

    [SerializeField, Range(0f, 1f)]
    private float attackVolume = 0.8f;

    [SerializeField, Min(0f)]
    private float soundCooldown = 0.15f;

    [NonSerialized]
    private float lastSoundTime = float.NegativeInfinity;

    public abstract void Execute(Transform origin);

    public abstract bool TryGetTarget(
        Transform origin,
        out Collider2D target
    );

    protected Vector2 GetDirection(
        Transform origin,
        float angle = 0f
    )
    {
        float rotation =
            origin.eulerAngles.z + angleOffset + angle;

        float radians = rotation * Mathf.Deg2Rad;

        return new Vector2(
            Mathf.Cos(radians),
            Mathf.Sin(radians)
        );
    }

    protected bool TryRaycast(
        Transform origin,
        Vector2 direction,
        out Collider2D target
    )
    {
        PlayAttackSound(origin);

        RaycastHit2D hit = Physics2D.Raycast(
            origin.position,
            direction,
            attackDistance,
            targetMask | obstacleMask
        );

        target = hit.collider;

        return target != null &&
            (targetMask.value &
            (1 << target.gameObject.layer)) != 0;
    }

    private void PlayAttackSound(Transform origin)
    {
        if (attackSounds == null ||
            attackSounds.Length == 0)
            return;

        if (Time.time - lastSoundTime < soundCooldown)
            return;

        AudioClip clip = attackSounds[
            UnityEngine.Random.Range(0, attackSounds.Length)
        ];

        if (clip == null)
            return;

        lastSoundTime = Time.time;

        AudioSource source =
            origin.GetComponentInParent<AudioSource>();

        if (source != null)
        {
            source.PlayOneShot(clip, attackVolume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(
                clip,
                origin.position,
                attackVolume
            );
        }
    }
}
