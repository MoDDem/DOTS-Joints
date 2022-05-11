using Unity.Entities;
using Unity.Mathematics;

public struct StartTag : IComponentData
{
    public bool UpdateArray;
    public bool UpdateMesh;
    public uint SegmentsCount;
}