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
using Material = UnityEngine.Material;

public class RopeSpawnerComponentView : MonoBehaviour, IConvertGameObjectToEntity
{
    public Material material;
    
    public GameObject startPoint;
    public int len = 1;
    public float speed;
    public float radius = 0.1f;
    public float3 offset = new float3(0, .5f, 0);
    
    //----Constraint settings
    public float3 incrementDirection;
    [Range(0.0001f, 10.0f)]
    public float frequencyHz = 5.0f;
    [Range(0.0001f, 10.0f)]
    public float halfLife = 0.02f;
    //----------------------
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<PairedSegmentsBuffer>(entity);
        
        dstManager.SetName(entity, "Start rope point");
        dstManager.AddComponentData(entity, new Translation { Value = startPoint.transform.position });
        
        var segmentType = dstManager.CreateArchetype(
            typeof(LocalToWorld),
            typeof(ConstraintComponent),
            typeof(PhysicsCollider),
            typeof(PhysicsMass),
            typeof(PhysicsVelocity),
            typeof(RenderBounds),
            typeof(PhysicsMassOverride)
        );

        var segmentEntities = new NativeArray<Entity>(len, Allocator.Persistent);

        dstManager.CreateEntity(segmentType, segmentEntities);
        
        for (int i = 0; i < len; i++)
        {
            float mass = 10f;
            
            var segment = segmentEntities[i];
            dstManager.SetName(segment,"Rope Segment " + i);

            var position = ((float3)startPoint.transform.position) + (incrementDirection * radius * (i+1));

            dstManager.AddComponentData(segment, new Translation {Value = position});
            dstManager.AddComponentData(segment, new Rotation{Value = quaternion.identity});
            dstManager.AddComponentData(segment, new LocalToWorld());

            dstManager.SetComponentData(segment, new PhysicsCollider
            {
                Value = Unity.Physics.SphereCollider.Create(new SphereGeometry
                {
                    Radius = radius,
                    Center = float3.zero
                })
            });

            var localCollider = dstManager.GetComponentData<PhysicsCollider>(segment);
            dstManager.SetComponentData(segment, PhysicsMass.CreateDynamic(localCollider.MassProperties, mass));
            //dstManager.SetComponentData(segment, new PhysicsMassOverride {IsKinematic = 1});

            float angularFrequency = frequencyHz * (2.0f * math.PI);
            dstManager.SetComponentData(segment, new ConstraintComponent
            {
                Constraint = segment,
                FrequencyHz = frequencyHz,
                DampingRatio = 0.6931472f / (angularFrequency * halfLife),
                AngularFrequency = angularFrequency,
                DampingCoefficient = 0,
                SpringConstant = 0,
                Direction = incrementDirection,
                Origin = (i-1 >= 0) ? segmentEntities[i-1] : entity,
                Mass = mass,
                Target = position,
                Radius = radius,
                Offset = offset
            });

            dstManager.GetBuffer<PairedSegmentsBuffer>(entity).Add(new PairedSegmentsBuffer { Value = segment });
        }

        Mesh mesh = new Mesh();
        mesh.MarkDynamic();
        mesh.name = "extendedMesh";

        dstManager.AddSharedComponentData(entity, new RenderMesh
        {
            mesh = mesh,
            material = material
        });
        dstManager.AddComponent(entity, typeof(RenderBounds));

        dstManager.AddComponentData(entity, new StartTag { UpdateArray = false, UpdateMesh = true, SegmentsCount = (uint) segmentEntities.Length });

        Destroy(startPoint);
        segmentEntities.Dispose();
    }
}
