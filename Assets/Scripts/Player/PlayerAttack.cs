using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    [SerializeField] private MeleeAttack meleeAttack = new();

    [Header("References")]
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private InputActionReference attackAction;

    private void Awake()
    {
        if (attackOrigin == null)
            attackOrigin = transform;
    }

    private void OnEnable()
    {
        attackAction.action.Enable();
    }

    private void OnDisable()
    {
        attackAction.action.Disable();
    }

    private void Update()
    {
        if (attackAction.action.WasPressedThisFrame())
        {
            meleeAttack.Execute(attackOrigin);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (meleeAttack == null)
            return;

        Transform origin = attackOrigin != null
            ? attackOrigin
            : transform;

        meleeAttack.DrawGizmos(origin);
    }
}
