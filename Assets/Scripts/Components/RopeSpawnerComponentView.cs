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
    public Mesh mesh;
    public Material material;
    
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
        dstManager.AddBuffer<PairSegmentsComponent>(entity);
        
        dstManager.SetName(entity, "Start rope point");
        dstManager.AddComponent(entity, typeof(StartTag));
        dstManager.AddComponentData(entity, new Translation {Value = startPoint.transform.position});

        var segmentType = dstManager.CreateArchetype(
            typeof(LocalToWorld),
            typeof(ConstraintComponent),
            typeof(PhysicsCollider),
            typeof(PhysicsMass),
            typeof(PhysicsVelocity),
            typeof(RenderMesh),
            typeof(RenderBounds)
        );
        var segmentEntities = new NativeArray<Entity>(len, Allocator.Persistent);
        dstManager.CreateEntity(segmentType, segmentEntities);
        
        for (int i = 0; i < len; i++)
        {
            float mass = 10f;
            
            var segment = segmentEntities[i];
            dstManager.SetName(segment,"Rope Segment " + i);

            var position = dstManager.GetComponentData<Translation>(entity).Value + (incrementDirection * (i + 1));
            
            dstManager.AddComponentData(segment, new Translation {Value = position});
            dstManager.AddComponentData(segment, new Rotation{Value = quaternion.identity});
            dstManager.AddComponentData(segment, new LocalToWorld());
            
            dstManager.SetComponentData(segment, new PhysicsCollider
            {
                Value = Unity.Physics.BoxCollider.Create(new BoxGeometry
                {
                    BevelRadius = 0.5f,
                    Center = float3.zero,
                    Orientation = quaternion.identity,
                    Size = new float3(1, 1, 1)
                }, CollisionFilter.Default, Unity.Physics.Material.Default)
            });
            dstManager.SetSharedComponentData(segment, new RenderMesh
            {
                mesh = mesh,
                material = material
            });
            
            var localCollider = dstManager.GetComponentData<PhysicsCollider>(segment);
            dstManager.SetComponentData(segment, PhysicsMass.CreateDynamic(localCollider.MassProperties, mass));

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
                Origin = (i-1 >= 0) ? segmentEntities[i-1] : entity,
                Mass = mass,
                Target = position
            });

            dstManager.GetBuffer<PairSegmentsComponent>(entity).Add(segment);
        }
        
        /*using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp))
        {
            ref SegmentsMapBlobAsset mapAsset = ref blobBuilder.ConstructRoot<SegmentsMapBlobAsset>();
            mapAsset.Height = len;
            mapAsset.Speed = speed;
            
            BlobBuilderArray<Entity> segmentArray = blobBuilder.Allocate(ref mapAsset.Map, len);
            for (int t = 0; t < len; t++)
            {
                segmentArray[t] = segmentEntities[t];
            }
            
            var assetReference = blobBuilder.CreateBlobAssetReference<SegmentsMapBlobAsset>(Allocator.Persistent);
            dstManager.AddComponentData(entity, new PairSegmentsComponent { SegmentsMap = assetReference });
        }*/
        
        Destroy(startPoint);
        segmentEntities.Dispose();
    }
}
