using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;

public struct PairConnectorComponent : IBufferElementData
{
    public static implicit operator Entity(PairConnectorComponent e) { return e.Value; }
    public static implicit operator PairConnectorComponent(Entity e) { return new PairConnectorComponent { Value = e }; }
    public Entity Value;
}
