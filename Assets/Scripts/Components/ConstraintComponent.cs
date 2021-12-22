using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics.Systems;
using Unity.Transforms;
using System;
using Unity.Mathematics;

[Serializable]
public struct ConstraintComponent : IComponentData
{
    public bool IsUpdated;
    public int OrderId;
    public float3 Direction;
    public float Mass;
    public float3 TotalLambda;
    public float3 Relative;
    public float3x3 EffectiveMass;
    public float3x3 Cross;
    public float Sbc;
    public float3 PositionErrorBias;

    public float3 Target;
    public float DampingCoefficient;
    public float DampingRatio;
    public float FrequencyHz;
    public float SpringConstant;
    public float AngularFrequency;
}

