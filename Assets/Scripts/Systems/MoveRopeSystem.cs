using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class MoveRopeSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var dt = Time.DeltaTime;
        float3 pos = new float3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if ((pos != float3.zero).x || (pos != float3.zero).z)
        {
            Entities.WithAll<MoveRopeComponent>().ForEach((Entity entity,
                ref MoveRopeComponent moveRope,
                ref Translation translation) =>
            {
                Debug.Log(pos);
                translation.Value += pos * dt * moveRope.Speed;;
            }).Schedule();
        }
        
        if(Input.GetAxis("Jump") > 0)
        {
            bool isAdded = false;
            
            Entities.ForEach((Entity entity,
                ref ConstraintComponent constraint) =>
            {
                Debug.Log(constraint.OrderId);
                /*
                if (constraint.OrderId == 1 && !isAdded)
                {
                    isAdded = true;
                    Debug.Log("new");
                    var nw = EntityManager.Instantiate(entity);
                    constraint.OrderId++;
                    GetBuffer<PairConnectorComponent>(entity).Clear();
                    GetBuffer<PairConnectorComponent>(entity).Add(nw);

                    constraint.IsUpdated = true;
                }
                else if(!constraint.IsUpdated)
                {
                    constraint.OrderId++;
                    constraint.IsUpdated = true;
                    constraint.Target += constraint.Direction;
                }*/
            }).Run();
        }
    }
}