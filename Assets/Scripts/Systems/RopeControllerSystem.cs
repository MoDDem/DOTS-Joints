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

        var bufferEntity = GetSingletonEntity<StartTag>();
        var getBuffer = GetBufferFromEntity<PairedSegmentsBuffer>(false);
        var getConstraintComponent = GetComponentDataFromEntity<ConstraintComponent>(true);

        var lenJob = moveJob;

        if(Input.GetKeyDown("space"))
        {
            lenJob = Entities.WithAll<ConstraintComponent>()
                .WithReadOnly(getConstraintComponent)
                .WithNativeDisableContainerSafetyRestriction(getBuffer)
                .ForEach((int entityInQueryIndex) =>
            {
                var array = getBuffer[bufferEntity].AsNativeArray();
                
                Entity entity = array[entityInQueryIndex];
                var constraint = getConstraintComponent[entity];
                
                if (constraint.Origin == bufferEntity)
                {
                    var newEntity = ecb.Instantiate(entityInQueryIndex, entity);
                    constraint.Constraint = newEntity;
                    ecb.SetComponent(entityInQueryIndex, newEntity, constraint);

                    constraint.Constraint = entity;
                    constraint.Origin = newEntity;
                    ecb.SetComponent(entityInQueryIndex, entity, constraint);
                    //ecb.SetComponent(entityInQueryIndex, entity, new PhysicsMassOverride{ IsKinematic = 1 }); // TODO
                    ecb.AppendToBuffer(array.Length, bufferEntity, new PairedSegmentsBuffer { Value = newEntity });
                }
            }).Schedule(moveJob);
            commandBuffer.AddJobHandleForProducer(lenJob);

            EntityManager.SetComponentData(bufferEntity, new StartTag { UpdateArray = true, UpdateMesh = false, SegmentsCount = (uint) getBuffer[bufferEntity].Length + 1 });
        }
        if(Input.GetKeyDown("left shift"))
        {
            lenJob = Entities.WithAll<ConstraintComponent>()
                .WithReadOnly(getConstraintComponent)
                .WithReadOnly(getBuffer)
                .ForEach((int entityInQueryIndex) =>
                {
                    var array = getBuffer[bufferEntity];
                
                    if(array.Length <= 1)
                        return;
                    
                    Entity entity = array[entityInQueryIndex];
                    var constraint = getConstraintComponent[entity];
                    
                    if (constraint.Origin == bufferEntity)
                    {
                        ecb.DestroyEntity(entityInQueryIndex, entity);
                    } else
                    {
                        var originConstraint = getConstraintComponent[constraint.Origin];
                        if (originConstraint.Origin == bufferEntity)
                        {
                            constraint.Origin = bufferEntity;
                            ecb.SetComponent(entityInQueryIndex, entity, constraint);   
                        }
                    }
                }).Schedule(moveJob);
            commandBuffer.AddJobHandleForProducer(lenJob);

            EntityManager.SetComponentData(bufferEntity, new StartTag { UpdateArray = true, UpdateMesh = false, SegmentsCount = (uint)getBuffer[bufferEntity].Length - 1 });
        }

        return lenJob;
    }
}
