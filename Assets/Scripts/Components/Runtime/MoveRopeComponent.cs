using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public enum Axis
{
    X,Y,Z
}

[Serializable]
public struct MoveRopeComponent : IComponentData
{
    public float3 Value;
    
    public float Split;

    public Axis Axis;

    public float Speed;
}
