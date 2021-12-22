using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(PhysicsBodyAuthoring), typeof(OnGameObjectConverted))]
public sealed class ConstraintComponentView : MonoBehaviour, IConvertGameObjectToEntity
{
    public float3 incrementDirection;
    public int orderID;
    public float3 target;
    [Range(0.0001f, 10.0f)]
    public float frequencyHz = 5.0f;
    /*[Range(0.0f, 10.0f)]
    public float dampingRatio = 0.5f;*/
    [Range(0.0001f, 10.0f)]
    public float halfLife = 0.02f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (!enabled)
            return;
        
        float angularFrequency = frequencyHz * (2.0f * math.PI);
        dstManager.AddComponentData(entity, new ConstraintComponent()
        {
            FrequencyHz = frequencyHz,
            DampingRatio = 0.6931472f / (angularFrequency * halfLife), // dampingRatio
            AngularFrequency = angularFrequency,
            DampingCoefficient = 0,
            SpringConstant = 0,
            Target = target,
            Direction = incrementDirection,
            OrderId = orderID
        });
    }
}
