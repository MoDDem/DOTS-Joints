using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class PairConnectorSystem : SystemBase
{
	protected override void OnUpdate()
	{
		Entities.ForEach((Entity entity,
			ref ConstraintComponent segment) =>
		{
			Entity origin = GetBuffer<PairConnectorComponent>(entity)[0].Value;
			segment.Target = GetComponent<Translation>(origin).Value + segment.Direction;
		}).Run();
	}
}
