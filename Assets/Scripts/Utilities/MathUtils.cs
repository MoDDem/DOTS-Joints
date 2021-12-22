using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public static class MathUtils
{
	public static float3x3 Skew(Vector3 v)
	{
		return
		  new float3x3
		  (
			  0.0f, -v.z, v.y,
			 v.z, 0.0f, -v.x,
			-v.y, v.x, 0.0f
		  );
	}

	public static void VelConstraintBias(float dampingCoefficient, float springConstant, float dt, out float posBiasCoef, out float softBiasCoef)
	{
		float hk = dt * springConstant;
		float gamma = dampingCoefficient + hk;
		if (gamma > 0.0f)
			gamma = 1.0f / gamma;

		float dtInv = 1.0f / dt;
		float beta = hk * gamma;

		posBiasCoef = beta * dtInv;
		softBiasCoef = gamma * dtInv;
	}

	public static float3x3 InverseInertialWs(float4x4 local, float3 inertia)
	{
		/*
		float3x3 _local = new(
			new float3(local.c0.x, local.c0.y, local.c0.z), 
			new float3(local.c1.x, local.c1.y, local.c1.z), 
			new float3(local.c2.x, local.c2.y, local.c2.z));

		float3 inversed = math.mul(math.mul(math.transpose(_local), inertia), _local);
		float3x3 a = new(inversed.x, inversed.y, inversed.z);
		*/
		float3x3 a = new(
				new float3(1, 0, 0),
				new float3(0, 1, 0),
				new float3(0, 0, 1)
			);
		//Debug.Log(a);
		/*
		var inertialWs =
				math.mul(math.transpose(local), new float4(inertia, 0));

		var a = new float3x3(math.mul(inertialWs, local).x, math.mul(inertialWs, local).y, math.mul(inertialWs, local).z);
		*/
		return a;
	}
	/*
	public static float3x3 InverseInertialWs(float4x4 local, float3 inertia)
	{
		float3x3 transformT = new float3x3(
			math.transform(local, new float3(1f, 0f, 0f)),
			math.transform(local, new float3(0f, 1f, 0f)),
			math.transform(local, new float3(0f, 0f, 1f)));
		return math.transpose(transformT) * inertia * transformT;
	}*/
}
