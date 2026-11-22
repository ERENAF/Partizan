using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class Rotation : MonoBehaviour,IPausable
{
    protected virtual float Rotate(Vector2 point, Vector2 target)
    {
        Vector2 direction = target - point;
        return Mathf.Atan2(direction.y,direction.x)*Mathf.Rad2Deg;
    }
}
