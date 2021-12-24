using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Physics.Authoring;

public class RopeSpawnerComponentView : MonoBehaviour, IConvertGameObjectToEntity
{
    /*
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
    */

    public GameObject startPoint;
    public int len = 1;
    public float speed;
    
    //----Constraint settings
    public float3 incrementDirection;
    [Range(0.0001f, 10.0f)]
    public float frequencyHz = 5.0f;
    [Range(0.0001f, 10.0f)]
    public float halfLife = 0.02f;
    //----------------------
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var startEntity = dstManager.CreateEntity();
        dstManager.AddComponentData(startEntity, new Translation {Value = startPoint.transform.position});
        dstManager.AddComponentData(startEntity, new MoveRopeComponent() {Speed = speed});
        Destroy(startPoint);

        var segmentType = dstManager.CreateArchetype(
            typeof(LocalToWorld),
            typeof(Translation),
            typeof(Rotation),
            typeof(ConstraintComponent),
            //typeof(PhysicsCollider),
            typeof(PhysicsMass),
            typeof(PhysicsVelocity),
            typeof(RenderMesh),
            typeof(RenderBounds)
        );
        var segmentEntities = new NativeArray<Entity>(len, Allocator.Temp);
        dstManager.CreateEntity(segmentType, segmentEntities);
        
        for (int i = 0; i < len; i++)
        {
            var segment = segmentEntities[i];
            dstManager.SetName(segment,"Rope Segment " + i);

            var position = dstManager.GetComponentData<Translation>(startEntity).Value + (incrementDirection * (i + 1));
            
            dstManager.SetComponentData(segmentEntities[i], new Translation
            {
                Value = position
            });
            dstManager.SetComponentData(segmentEntities[i], new PhysicsMass
            {
                InverseMass = 0.1f,
                InverseInertia = new float3(10,10,10)
            });
            dstManager.SetComponentData(segmentEntities[i], new Rotation
            {
                Value = quaternion.identity
            });
            dstManager.SetComponentData(segmentEntities[i], new PhysicsVelocity
            {
                Angular = float3.zero,
                Linear = float3.zero
            });
            dstManager.SetComponentData(segmentEntities[i], new LocalToWorld()
            {
                Value = new float4x4(float3x3.zero, position)
            });
            
            float angularFrequency = frequencyHz * (2.0f * math.PI);
            dstManager.SetComponentData(segment, new ConstraintComponent
            {
                FrequencyHz = frequencyHz,
                DampingRatio = 0.6931472f / (angularFrequency * halfLife),
                AngularFrequency = angularFrequency,
                DampingCoefficient = 0,
                SpringConstant = 0,
                Direction = incrementDirection,
                OrderId = i,
                Origin = (i-1 >= 0) ? segmentEntities[i-1] : startEntity,
                Mass = math.pow(dstManager.GetComponentData<PhysicsMass>(segmentEntities[i]).InverseMass, -1),
                Target = position
            });
        }
        
        dstManager.DestroyEntity(entity);
    }
}
