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
public class RopeControllerSystem : JobComponentSystem
{
    [NativeDisableParallelForRestriction]
    private EndFixedStepSimulationEntityCommandBufferSystem commandBuffer;
    protected override void OnCreate()
    {
        commandBuffer = World.DefaultGameObjectInjectionWorld.GetExistingSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var ecb = commandBuffer.CreateCommandBuffer().AsParallelWriter();
        var dt = Time.DeltaTime;
        
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
            Debug.Log(Time.ElapsedTime + " et");
            
            var bufferEntity = GetSingletonEntity<StartTag>();
            var getBuffer = GetBufferFromEntity<PairSegmentsComponent>(true);
            var getConstraintComponent = GetComponentDataFromEntity<ConstraintComponent>(true);

            lenJob = Entities.WithAll<ConstraintComponent>()
                .WithReadOnly(getConstraintComponent)
                .WithReadOnly(getBuffer)
                .ForEach((int entityInQueryIndex) =>
            {
                var array = getBuffer[bufferEntity];
                
                Entity entity = array[entityInQueryIndex];
                var constraint = getConstraintComponent[entity];
                
                if (constraint.OrderId == 0)
                {
                    var newEntity = ecb.Instantiate(entityInQueryIndex, entity);

                    constraint.OrderId++;
                    constraint.Origin = newEntity;
                    ecb.SetComponent(entityInQueryIndex, entity, constraint);
                    
                    ecb.AppendToBuffer<PairSegmentsComponent>(array.Length, bufferEntity, newEntity);
                }

                if (constraint.OrderId > 0)
                {
                    constraint.OrderId++;
                    ecb.SetComponent(entityInQueryIndex, entity, constraint);   
                }
            }).Schedule(moveJob);
            commandBuffer.AddJobHandleForProducer(lenJob);
        }
        
        return lenJob;
        //return inputDeps;
    }
}

//TODO: https://www.youtube.com/watch?v=nuxTq0AQAyY