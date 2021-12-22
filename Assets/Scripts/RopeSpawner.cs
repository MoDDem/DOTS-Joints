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
        manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        startFrom.AddComponent(typeof(OnGameObjectConverted));
	}

    private IEnumerator SpawnRope()
    {
        if (i == segmentsCount - 1)
            segment.GetComponent<PhysicsBodyAuthoring>().Mass = 100000f;

        segment.transform.position = nextPos;
        segment.GetComponent<ConstraintComponentView>().target = nextPos;
        segment.GetComponent<ConstraintComponentView>().orderID = i;
        segment.GetComponent<ConstraintComponentView>().incrementDirection = incrementDirection;
        nextPos += incrementDirection;

        yield return new WaitForFixedUpdate();
        
        Instantiate(segment).name = "Cube " + i;
        i = i + 1;
    }

	public void Converted(Entity entity, GameObject gObject)
	{
        if (gObject == startFrom)
        {
            prevOrigin = entity;
            nextPos = manager.GetComponentData<Translation>(entity).Value + incrementDirection;
            
            segment.GetComponent<PhysicsBodyAuthoring>().Mass = 10f;
            StartCoroutine(SpawnRope());
        }
            
        manager.AddBuffer<PairConnectorComponent>(entity).Add(prevOrigin);
        
        prevOrigin = entity;

        if (i >= segmentsCount)
            return;

		StartCoroutine(SpawnRope());
	}
}
