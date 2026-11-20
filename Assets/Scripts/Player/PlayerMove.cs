using System;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private Speed speed = new();
    [SerializeField, Min(0)] private float moveSpeed;
    [SerializeField, Min(0)] private float sneakSpeed;
    [SerializeField,Min(0)] private float sprintSpeed;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference sneakAction;
    [SerializeField] private InputActionReference sprintAction;

    private Rigidbody2D rb;
    private Vector3 direction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
    }

    private float SpeedMultiplier()
    {
        if (sneakAction.action.IsPressed()) return sneakSpeed;
        if (sprintAction.action.IsPressed()) return sprintSpeed;
        return 1f;
    }

    private void Update()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();
        input = Vector2.ClampMagnitude(input,1f);

        if (input.sqrMagnitude > 0.0001f)
        {
            direction = new Vector3(input.x,input.y,0).normalized;
        }

        float targetSpeed = moveSpeed*SpeedMultiplier()*input.magnitude;
        float currentSpeed = speed.Update(targetSpeed,Time.deltaTime);

        rb.linearVelocity = direction * currentSpeed;
    }
}
