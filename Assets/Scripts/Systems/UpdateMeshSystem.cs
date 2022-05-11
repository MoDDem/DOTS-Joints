using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

public class UpdateMeshSystem : JobComponentSystem
{
	struct ComputeUVs : IJobFor
	{
        Vector2[] UVs;
        int Sides;
        int PosCount;

		public void Execute(int segment)
		{
            for (int side = 0; side < Sides; side++)
            {
                var vertIndex = (segment * Sides + side);
                var u = side / (Sides - 1f);
                var v = segment / (PosCount - 1f);

                UVs[vertIndex] = new Vector2(u, v);
            }
        }
	}

    struct ComputeIndices : IJobFor
    {
        int Sides;
        int[] indices;
        int currentIndicesIndex; //0

        public void Execute(int segment)
        {
            for (int side = 0; side < Sides; side++)
            {
                var vertIndex = (segment * Sides + side);
                var prevVertIndex = vertIndex - Sides;

                // Triangle one
                indices[currentIndicesIndex++] = prevVertIndex;
                indices[currentIndicesIndex++] = (side == Sides - 1) ? (vertIndex - (Sides - 1)) : (vertIndex + 1);
                indices[currentIndicesIndex++] = vertIndex;


                // Triangle two
                indices[currentIndicesIndex++] = (side == Sides - 1) ? (prevVertIndex - (Sides - 1)) : (prevVertIndex + 1);
                indices[currentIndicesIndex++] = (side == Sides - 1) ? (vertIndex - (Sides - 1)) : (vertIndex + 1);
                indices[currentIndicesIndex++] = prevVertIndex;
            }
        }
    }

    struct ComputeCircle : IJob
    {
        int Sides;
        float3[] Positions;
        int index;
        int Radius;

        public void Execute()
        {
            var dirCount = 0;
            var forward = Vector3.zero;

            // If not first index
            if (index > 0)
            {
                forward += (Vector3)math.normalize(Positions[index] - Positions[index - 1]);
                dirCount++;
            }

            // If not last index
            if (index < Positions.Length - 1)
            {
                forward += (Vector3)math.normalize(Positions[index + 1] - Positions[index]);
                dirCount++;
            }

            // Forward is the average of the connecting edges directions
            forward = (forward / dirCount).normalized;
            var side = Vector3.Cross(forward, forward + new Vector3(.123564f, .34675f, .756892f)).normalized;
            var up = Vector3.Cross(forward, side).normalized;

            var circle = new Vector3[Sides];
            var angle = 0f;
            var angleStep = (2 * Mathf.PI) / Sides;

            var t = index / (Positions.Length - 1f);
            var radius = Radius;

            for (int i = 0; i < Sides; i++)
            {
                var x = Mathf.Cos(angle);
                var y = Mathf.Sin(angle);

                circle[i] = (Vector3)Positions[index] + side * x * radius + up * y * radius;

                angle += angleStep;
            }
        }
    }

    RenderMesh renderer;
    MeshBuilder3 builder;

	protected override JobHandle OnUpdate(JobHandle inputDeps)
	{
        if (renderer.mesh == null)
            renderer = EntityManager.GetSharedComponentData<RenderMesh>(GetSingletonEntity<StartTag>());
        var start = GetSingletonEntity<StartTag>();
        var buff = EntityManager.GetBuffer<PairedSegmentsBuffer>(start);
        var startTag = EntityManager.GetComponentData<StartTag>(start);
        if (startTag.UpdateMesh)
		{
            float3[] pos = new float3[buff.Length];
			for (int i = 0; i < buff.Length; i++)
			    pos[i] = EntityManager.GetComponentData<Translation>(buff[i].Value).Value - EntityManager.GetComponentData<Translation>(start).Value;
            builder = builder ?? new MeshBuilder3(pos, renderer.mesh);
            builder._Positions = pos;
            builder.RenderCable();
		}

        /*
        var arch = EntityManager.GetChunk(buff[0]);
        var a = new SortSegments {
            segments = buff.ToNativeArray(Allocator.TempJob),
            em = EntityManager,
            isComplete = false
        };
        var sortedPoints = a.ScheduleParallel(arch.ChunkEntityCount - 1, 32, inputDeps);
        Debug.Log(sortedPoints.IsCompleted);*/
        
        /*
        float3[] _Positions;

        int _Sides = 8;
        float _Radius = 0.1f;
        Vector3[] _Vertices;
        Mesh _Mesh = renderer.mesh;

        if (_Mesh == null || _Positions == null || _Positions.Length <= 1)
            return;

        // Tube
        var verticesLength = _Sides * _Positions.Length;
        if (_Vertices == null || _Vertices.Length != verticesLength)
        {
            _Vertices = new Vector3[verticesLength];

            var indices = GenerateIndices();
            var uvs = GenerateUVs();

            if (verticesLength > _Mesh.vertexCount)
            {
                _Mesh.vertices = _Vertices;
                _Mesh.triangles = indices;
                _Mesh.uv = uvs;
            }
            else
            {
                _Mesh.triangles = indices;
                _Mesh.vertices = _Vertices;
                _Mesh.uv = uvs;
            }
        }

        var currentVertIndex = 0;

        for (int i = 0; i < _Positions.Length; i++)
        {
            var circle = CalculateCircle(i);
            foreach (var vertex in circle)
            {

                _Vertices[currentVertIndex++] = vertex;
            }
        }

        _Mesh.vertices = _Vertices;
        _Mesh.RecalculateNormals();
        _Mesh.RecalculateBounds();
        */
        return default;
    }
}
