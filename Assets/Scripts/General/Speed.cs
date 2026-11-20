using System;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

[Serializable]
public class Speed
{
    [SerializeField, Min(0)] private float acceleration;
    [SerializeField, Min(0)] private float deceleration;

    public float Current{get;private set;}

    public float Update(float target, float deltaTime)
    {
        float rate = target > Current ? acceleration : deceleration;
        Current = Mathf.MoveTowards(Current,target, rate*deltaTime);
        return Current;
    }

    public void Reset() => Current =0f;
}
