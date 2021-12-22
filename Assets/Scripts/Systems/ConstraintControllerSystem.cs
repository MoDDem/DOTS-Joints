using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using UnityEditor;
using Unity.Physics.Authoring;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class ConstraintControllerSystem : SystemBase
{
	protected override void OnUpdate()
	{
		float dt = Time.DeltaTime;

		Entities.ForEach((Entity entity,
			ref Translation position,
			ref Rotation rotation,
			ref ConstraintComponent segment,
			ref PhysicsMass physicsMass,
			ref PhysicsVelocity vel,
			ref LocalToWorld world) =>
		{
			if (segment.Mass == 0)
				segment.Mass = math.pow(physicsMass.InverseMass, -1);

			segment.DampingCoefficient = 2.0f * segment.Mass * segment.DampingRatio * segment.AngularFrequency;
			segment.SpringConstant = segment.Mass * segment.AngularFrequency * segment.AngularFrequency;
			MathUtils.VelConstraintBias(segment.DampingCoefficient, segment.SpringConstant, dt, out float pbc, out float m_sbc);

			segment.Relative = math.mul(rotation.Value, float3.zero - physicsMass.CenterOfMass);
			segment.Cross = MathUtils.Skew(-segment.Relative);
			
			float3 cPos = (position.Value + segment.Relative) - segment.Target;
			float3x3 k = physicsMass.InverseMass * float3x3.identity;

			//k += segment.Cross * MathUtils.InverseInertialWs(world.Value, physicsMass.InverseInertia) * math.transpose(segment.Cross); //if rotation is enabled
			k += m_sbc * float3x3.identity;

			segment.PositionErrorBias = pbc * cPos;
			segment.EffectiveMass = math.inverse(k);

			// TODO: warm starting
			segment.TotalLambda = float3.zero;
			float3 cVel = vel.Linear + math.cross(vel.Angular, segment.Relative);
			float3 jvb = cVel + segment.PositionErrorBias + segment.Sbc * segment.TotalLambda;
			float3 lambda = math.mul(segment.EffectiveMass, -jvb);
			segment.TotalLambda += lambda;
			/*
			foreach (var item in typeof(RopeSegmentComponent).GetProperties())
				Debug.Log(item.Name + " - " + item.GetValue(segment).ToString());
			*/
			vel.Linear += physicsMass.InverseMass * lambda;
			vel.Angular = float3.zero;
			rotation.Value = quaternion.identity;
			//vel.Angular += math.mul(MathUtils.InverseInertialWs(world.Value, physicsMass.InverseInertia), math.mul(math.transpose(segment.Cross), lambda));
		}).Run();
	}
}
