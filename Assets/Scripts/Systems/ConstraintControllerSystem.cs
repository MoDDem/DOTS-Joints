using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public class ConstraintControllerSystem : SystemBase
{
	protected override void OnUpdate()
	{
		float dt = Time.DeltaTime;

		Entities.ForEach((Entity entity,
			ref Rotation rotation,
			ref ConstraintComponent segment,
			ref PhysicsMass physicsMass,
			ref PhysicsVelocity vel) =>
		{
			if(segment.Origin == Entity.Null)
				return;

			var position = GetComponent<Translation>(entity);
			segment.Target = GetComponent<Translation>(segment.Origin).Value + (segment.Radius + segment.Offset) * segment.Direction;
			
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

			vel.Linear += physicsMass.InverseMass * lambda;
			vel.Angular = float3.zero;
			rotation.Value = quaternion.identity;
			//vel.Angular += math.mul(MathUtils.InverseInertialWs(world.Value, physicsMass.InverseInertia), math.mul(math.transpose(segment.Cross), lambda));
		}).Run();

		//TODO: think about how to clear the array in better way
		var start = GetSingletonEntity<StartTag>();
		var startTag = EntityManager.GetComponentData<StartTag>(start);
		var _array = EntityManager.GetBuffer<PairedSegmentsBuffer>(start);

		if (startTag.UpdateArray || startTag.SegmentsCount != _array.Length)
		{
			var pos = EntityManager.GetComponentData<Translation>(start);

			for (int i = 0; i < _array.Length; i++)
			{
				if (!EntityManager.Exists(_array[i]))
					_array.RemoveAt(i);
			}

			var len = _array.Length;
			for (var i = 1; i < len; i++)
			{
				for (var j = 0; j < len - i; j++)
				{
					var point1 = EntityManager.GetComponentData<Translation>(_array[j]).Value;
					var point2 = EntityManager.GetComponentData<Translation>(_array[j + 1]).Value;
					if (math.distance(pos.Value, point1) > math.distance(pos.Value, point2))
					{
						(_array[j], _array[j + 1]) = (_array[j + 1], _array[j]);
					}
				}
			}

			EntityManager.SetComponentData(start, new StartTag { UpdateArray = false, UpdateMesh = true });
		}
	}
}
