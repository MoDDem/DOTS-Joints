using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

[Serializable]
public struct SplineMeshDataComponent : IComponentData
{
    public RenderMesh RenderMesh;
    public Mesh Mesh;
    public Material Material;

    public float3 Translation;
    public float3 Scale;
    public quaternion Rotation;

    public bool CurveSpace;
}
