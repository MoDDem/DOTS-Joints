using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(8)]
public struct PairSegmentsComponent : IBufferElementData
{
    public static implicit operator Entity(PairSegmentsComponent e) { return e.Value; }
    public static implicit operator PairSegmentsComponent(Entity e) { return new PairSegmentsComponent { Value = e }; }
    public Entity Value;
}
