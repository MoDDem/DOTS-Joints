using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Physics.Authoring;

public class RopeSpawner : MonoBehaviour, IConvertedEntityTarget
{
    //private EndSimulationEntityCommandBufferSystem endSimulationSystem;
    private EntityManager manager;
    
    public GameObject segment;
    public GameObject startFrom;
    
    public int segmentsCount = 1;
    [HideInInspector]
    public float3 nextPos;
    public float3 incrementDirection;

    private int i = 0;
    private Entity prevOrigin;

    void Awake()
    {
        //endSimulationSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        startFrom.AddComponent(typeof(OnGameObjectConverted));
	}

    private IEnumerator SpawnRope(int id, Entity origin)
    {
        if (id == segmentsCount - 1)
            segment.GetComponent<PhysicsBodyAuthoring>().Mass = 100000f;

        segment.transform.position = nextPos;
        segment.GetComponent<ConstraintComponentView>().target = nextPos;
        segment.GetComponent<ConstraintComponentView>().orderID = id;
        segment.GetComponent<ConstraintComponentView>().incrementDirection = incrementDirection;
        nextPos += incrementDirection;
        
        yield return new WaitForFixedUpdate();
        Instantiate(segment).name = "Cube " + id;
    }

	public void Converted(Entity entity, GameObject gObject)
    {
        //var ecb = endSimulationSystem.CreateCommandBuffer().AsParallelWriter();
        
        if (gObject == startFrom)
        {
            prevOrigin = entity;
            nextPos = manager.GetComponentData<Translation>(entity).Value + incrementDirection;
            
            segment.GetComponent<PhysicsBodyAuthoring>().Mass = 10f;
            
            StartCoroutine(SpawnRope(0, Entity.Null));
            return;
        }
        
        //manager.AddBuffer<PairConnectorComponent>(entity).Add(prevOrigin);
        //ecb.AddBuffer<PairConnectorComponent>(i, entity).Add(prevOrigin);
        
        if (i >= segmentsCount)
            return;
        i++;
		StartCoroutine(SpawnRope(i, prevOrigin));
        
        prevOrigin = entity;
	}
}
