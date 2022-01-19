using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(8)]
public struct PairedSegmentsBuffer : IBufferElementData
{
    public static implicit operator Entity(PairedSegmentsBuffer e) { return e.Value; }
    public static implicit operator PairedSegmentsBuffer(Entity e) { return new PairedSegmentsBuffer { Value = e }; }
    public Entity Value;
}
