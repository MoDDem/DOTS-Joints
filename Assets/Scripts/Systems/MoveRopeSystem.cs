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
                translation.Value += pos * dt * moveRope.Speed;
            }).Schedule();
        }
        
        if(Input.GetAxis("Jump") > 0)
        {
            Entities.WithAll<ConstraintComponent>().ForEach((Entity entity) =>
            {
                /*
                var constraint = GetComponent<ConstraintComponent>(entity);
                
                if (constraint.OrderId == 0)
                {
                    var newEntity = EntityManager.Instantiate(entity);
                    SetComponent(newEntity, new ConstraintComponent { OrderId = 0 });
                    SetComponent(entity, new ConstraintComponent { OrderId = constraint.OrderId + 1 });
                    GetBuffer<PairConnectorComponent>(entity).Clear();
                    GetBuffer<PairConnectorComponent>(entity).Add(newEntity);
                }

                if (constraint.OrderId > 0)
                    SetComponent(entity, new ConstraintComponent { OrderId = constraint.OrderId + 1 });*/
            }).Run();
        }
    }
}