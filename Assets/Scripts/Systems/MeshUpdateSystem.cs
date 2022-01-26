using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class MeshUpdateSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var meshData = GetSingleton<SplineMeshDataComponent>();
        
    }
    
    struct MeshModJob : IJobParallelFor
    {
        public static NativeArray<float3> vertices = new NativeArray<float3>();
        public static NativeArray<float3> normals = new NativeArray<float3>();
        public static NativeArray<float4> tangents = new NativeArray<float4>();
        public static NativeArray<float2> uvs = new NativeArray<float2>();
        //private List<Color> vertColors = new List<Color>();
        public static NativeArray<int> tris = new NativeArray<int>();

        static float vCoord = -1 * 10 * 1; // v texture coordinate.
        static float actualToRestLengthRatio = 0.25f;

        public void Execute(int sectionIndex)
        {
            bool normalizeV = false;
            
            float2 uv = float2.zero;
            float4 texTangent = float4.zero;
            float3 normal = float3.zero;
            float3 vertex = float3.zero;
            
            var uvScale = new float3(1, 1, 1);
            
            int prevIndex = Mathf.Max(sectionIndex - 1, 0);
            
            vCoord += uvScale.y * (Vector3.Distance(curve.Data[i].position, curve.Data[prevIndex].position) /
                                                   (normalizeV ? 1 : actualToRestLengthRatio));

            // calculate section thickness and scale the basis vectors by it:
            float sectionThickness = curve.Data[i].thickness * thicknessScale;

            // Loop around each segment:
            int nextSectionIndex = sectionIndex + 1;
            for (int j = 0; j <= 10; ++j)
            {
                // make just one copy of the section vertex:
                Vector2 sectionVertex = section.vertices[j];

                // calculate normal using section vertex, curve normal and binormal:
                normal.x = (sectionVertex.x * curve.Data[i].normal.x + sectionVertex.y * curve.Data[i].binormal.x) * sectionThickness;
                normal.y = (sectionVertex.x * curve.Data[i].normal.y + sectionVertex.y * curve.Data[i].binormal.y) * sectionThickness;
                normal.z = (sectionVertex.x * curve.Data[i].normal.z + sectionVertex.y * curve.Data[i].binormal.z) * sectionThickness;

                // offset curve position by normal:
                vertex.x = curve.Data[i].position.x + normal.x;
                vertex.y = curve.Data[i].position.y + normal.y;
                vertex.z = curve.Data[i].position.z + normal.z;

                // cross(normal, curve tangent)
                texTangent.x = normal.y * curve.Data[i].tangent.z - normal.z * curve.Data[i].tangent.y;
                texTangent.y = normal.z * curve.Data[i].tangent.x - normal.x * curve.Data[i].tangent.z;
                texTangent.z = normal.x * curve.Data[i].tangent.y - normal.y * curve.Data[i].tangent.x;
                texTangent.w = -1;

                uv.x = (j / (float) 10) * uvScale.x;
                uv.y = vCoord;

                vertices.Add(vertex);
                normals.Add(normal);
                tangents.Add(texTangent);
                //vertColors.Add(curve.Data[i].color);
                uvs.Add(uv);

                if (j < sectionSegments && i < curve.Count - 1)
                {
                    tris.Add(sectionIndex * 6 + j);
                    tris.Add(nextSectionIndex * 6 + j);
                    tris.Add(sectionIndex * 6 + (j + 1));

                    tris.Add(sectionIndex * 6 + (j + 1));
                    tris.Add(nextSectionIndex * 6 + j);
                    tris.Add(nextSectionIndex * 6 + (j + 1));
                }
            }
        }
    }
}
