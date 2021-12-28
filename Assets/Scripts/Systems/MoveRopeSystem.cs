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
using Unity.Rendering;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class MoveRopeSystem : JobComponentSystem
{
    [NativeDisableParallelForRestriction]
    private EndSimulationEntityCommandBufferSystem commandBuffer;
    protected override void OnCreate()
    {
        commandBuffer = World.DefaultGameObjectInjectionWorld.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var ecb = commandBuffer.CreateCommandBuffer().AsParallelWriter();
        var dt = Time.DeltaTime;
        var getConstraintComponent = GetComponentDataFromEntity<ConstraintComponent>(true);


        var bufferEntity = GetSingletonEntity<StartTag>();
        var getBuffer = GetBufferFromEntity<PairSegmentsComponent>();
        // var array = EntityManager
        //     .GetBuffer<PairSegmentsComponent>(GetSingletonEntity<StartTag>())
        //     .ToNativeArray(Allocator.TempJob);

        var moveJob = inputDeps;
        float3 pos = new float3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if ((pos != float3.zero).x || (pos != float3.zero).z)
        {
            moveJob = Entities.WithAll<StartTag>().ForEach((Entity entity,
                ref Translation translation) =>
            {
                translation.Value += pos * dt * 5;
            }).Schedule(inputDeps);
        }

        var lenJob = moveJob;
        if(Input.GetAxis("Jump") > 0)
        {
            lenJob = Entities.WithAll<ConstraintComponent>().WithReadOnly(getConstraintComponent).ForEach((Entity e, int entityInQueryIndex) =>
            {
                var array = getBuffer[bufferEntity];
                
                Entity entity = array[entityInQueryIndex];
                var constraint = getConstraintComponent[entity];
                
                if (constraint.OrderId == 0)
                {
                    var newEntity = ecb.Instantiate(array.Length, entity);
                    
                    ecb.AddComponent(array.Length - 1, newEntity, new ConstraintComponent
                    {
                        OrderId = 0, 
                        Origin = constraint.Origin
                    });
                    
                    ecb.AddComponent(entityInQueryIndex, entity, new ConstraintComponent
                    {
                        OrderId = constraint.OrderId + 1, 
                        Origin = newEntity
                    });

                    array.Add(newEntity);
                    array[entityInQueryIndex] = entity;
                }
                
                if (constraint.OrderId > 0)
                    ecb.AddComponent(entityInQueryIndex, entity, new ConstraintComponent { OrderId = constraint.OrderId + 1 });
            }).Schedule(moveJob);
        }
        
        commandBuffer.AddJobHandleForProducer(lenJob);
        return lenJob;
        return default;
        //return inputDeps;
    }
}

//TODO: https://www.youtube.com/watch?v=nuxTq0AQAyY