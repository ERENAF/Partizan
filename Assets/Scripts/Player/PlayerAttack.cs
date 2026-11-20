using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Attack meleeAttack;
    [SerializeField] private Attack rangeAttack;

    [SerializeField] private InputActionReference meleeAction;
    [SerializeField] private InputActionReference rangeAction;

    private void OnEnable()
    {
        meleeAction.action.Enable();
        rangeAction.action.Enable();
    }

    private void OnDisable()
    {
        meleeAction.action.Disable();
        rangeAction.action.Disable();
    }

    private void Update()
    {
        if (meleeAction.action.WasPressedThisFrame())
            meleeAttack.Execute();

        if (rangeAction.action.WasPressedThisFrame())
            rangeAttack.Execute();
    }
}
