using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MeshBuilder3
{
    private float3[] _Positions;

    [SerializeField] private int _Sides = 8;
    [SerializeField] private float _Radius = 0.1f;
    private Vector3[] _Vertices;
    private Mesh _Mesh;

    public MeshBuilder3(float3[] _points, Mesh _mesh = null)
	{
        _Positions = _points;
        _Mesh = _mesh;
	}

    public void RenderCable()
    {
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
    }

    Vector2[] GenerateUVs()
    {
        var uvs = new Vector2[_Positions.Length * _Sides];

        for (int segment = 0; segment < _Positions.Length; segment++)
        {
            for (int side = 0; side < _Sides; side++)
            {
                var vertIndex = (segment * _Sides + side);
                var u = side / (_Sides - 1f);
                var v = segment / (_Positions.Length - 1f);

                uvs[vertIndex] = new Vector2(u, v);
            }
        }

        return uvs;
    }

    int[] GenerateIndices()
    {
        // Two triangles and 3 vertices
        var indices = new int[_Positions.Length * _Sides * 2 * 3];

        var currentIndicesIndex = 0;
        for (int segment = 1; segment < _Positions.Length; segment++)
        {
            for (int side = 0; side < _Sides; side++)
            {
                var vertIndex = (segment * _Sides + side);
                var prevVertIndex = vertIndex - _Sides;

                // Triangle one
                indices[currentIndicesIndex++] = prevVertIndex;
                indices[currentIndicesIndex++] = (side == _Sides - 1) ? (vertIndex - (_Sides - 1)) : (vertIndex + 1);
                indices[currentIndicesIndex++] = vertIndex;


                // Triangle two
                indices[currentIndicesIndex++] = (side == _Sides - 1) ? (prevVertIndex - (_Sides - 1)) : (prevVertIndex + 1);
                indices[currentIndicesIndex++] = (side == _Sides - 1) ? (vertIndex - (_Sides - 1)) : (vertIndex + 1);
                indices[currentIndicesIndex++] = prevVertIndex;
            }
        }

        return indices;
    }

    Vector3[] CalculateCircle(int index)
    {
        var dirCount = 0;
        var forward = Vector3.zero;

        // If not first index
        if (index > 0)
        {
            forward += (Vector3)math.normalize(_Positions[index] - _Positions[index - 1]);
            dirCount++;
        }

        // If not last index
        if (index < _Positions.Length - 1)
        {
            forward += (Vector3)math.normalize(_Positions[index + 1] - _Positions[index]);
            dirCount++;
        }

        // Forward is the average of the connecting edges directions
        forward = (forward / dirCount).normalized;
        var side = Vector3.Cross(forward, forward + new Vector3(.123564f, .34675f, .756892f)).normalized;
        var up = Vector3.Cross(forward, side).normalized;

        var circle = new Vector3[_Sides];
        var angle = 0f;
        var angleStep = (2 * Mathf.PI) / _Sides;

        var t = index / (_Positions.Length - 1f);
        var radius = _Radius;

        for (int i = 0; i < _Sides; i++)
        {
            var x = Mathf.Cos(angle);
            var y = Mathf.Sin(angle);

            circle[i] = (Vector3)_Positions[index] + side * x * radius + up * y * radius;

            angle += angleStep;
        }

        return circle;
    }
}