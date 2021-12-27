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
    private EndSimulationEntityCommandBufferSystem commandBuffer;
    protected override void OnCreate()
    {
        commandBuffer = World.DefaultGameObjectInjectionWorld.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var dt = Time.DeltaTime;
        DynamicBuffer<PairSegmentsComponent> array = EntityManager.GetBuffer<PairSegmentsComponent>(GetSingletonEntity<StartTag>());
        var ecb = commandBuffer.CreateCommandBuffer().AsParallelWriter();
        
        float3 pos = new float3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if ((pos != float3.zero).x || (pos != float3.zero).z)
        {
            Entities.WithAll<StartTag>().ForEach((Entity entity,
                ref Translation translation) =>
            {
                translation.Value += pos * dt * 5;
            }).Schedule(inputDeps);
        }
        
        if(Input.GetAxis("Jump") > 0)
        {
            Entities.WithAll<ConstraintComponent>().ForEach((Entity e, int entityInQueryIndex) =>
            {
                Entity entity = array[entityInQueryIndex];
                var constraint = EntityManager.GetComponentData<ConstraintComponent>(entity);

                if (constraint.OrderId == 0)
                {
                    var newEntity = ecb.Instantiate(array.Length, entity);
                    
                    EntityManager.SetComponentData(newEntity, new ConstraintComponent
                    {
                        OrderId = 0, 
                        Origin = constraint.Origin
                    });
                    
                    EntityManager.SetComponentData(entity, new ConstraintComponent
                    {
                        OrderId = constraint.OrderId + 1, 
                        Origin = newEntity
                    });

                    array.Add(newEntity);
                    array[entityInQueryIndex] = entity;
                }
                
                if (constraint.OrderId > 0)
                    EntityManager.SetComponentData(entity, new ConstraintComponent { OrderId = constraint.OrderId + 1 });
            }).Schedule(inputDeps);
            
            commandBuffer.AddJobHandleForProducer(inputDeps);
            
            return OnUpdate().Schedule(this, inputDeps);
        }
    }
    
    [RequireComponentTag(typeof(ConstraintComponent))]
    private struct ChangeRopeLength : IJobChunk
    {
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            throw new NotImplementedException();
        }
    }
}

//TODO: https://www.youtube.com/watch?v=nuxTq0AQAyY