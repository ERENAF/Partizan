using UnityEditor.Search;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerRotation : Rotation
{
    [Range(-180,180)]
    [SerializeField] private float angleOffset;
    [SerializeField] private Camera playerCamera;

    private void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 target = playerCamera.ScreenToWorldPoint(mousePos);

        float angle = Rotate(transform.position,target);

        transform.rotation = Quaternion.Euler(0f,0f,angle+angleOffset);
    }
}
