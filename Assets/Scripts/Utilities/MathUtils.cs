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

	public static float3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd, out float mu, bool clampToSegment = true)
	{
		Vector3 ap = point - lineStart;
		Vector3 ab = lineEnd - lineStart;

		mu = Vector3.Dot(ap, ab) / Vector3.Dot(ab, ab);

		if (clampToSegment)
			mu = Mathf.Clamp01(mu);

		return lineStart + ab * mu;
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

	public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
	{

		closestPointLine1 = Vector3.zero;
		closestPointLine2 = Vector3.zero;

		float a = Vector3.Dot(lineVec1, lineVec1);
		float b = Vector3.Dot(lineVec1, lineVec2);
		float e = Vector3.Dot(lineVec2, lineVec2);

		float d = a * e - b * b;

		//lines are not parallel
		if (d != 0.0f)
		{

			Vector3 r = linePoint1 - linePoint2;
			float c = Vector3.Dot(lineVec1, r);
			float f = Vector3.Dot(lineVec2, r);

			float s = (b * f - c * e) / d;
			float t = (a * f - c * b) / d;

			closestPointLine1 = linePoint1 + lineVec1 * s;
			closestPointLine2 = linePoint2 + lineVec2 * t;

			return true;
		}

		else
		{
			return false;
		}
	}

	public static float DotProductAngle(Vector3 vec1, Vector3 vec2)
	{

		double dot;
		double angle;

		//get the dot product
		dot = Vector3.Dot(vec1, vec2);

		//Clamp to prevent NaN error. Shouldn't need this in the first place, but there could be a rounding error issue.
		if (dot < -1.0f)
		{
			dot = -1.0f;
		}
		if (dot > 1.0f)
		{
			dot = 1.0f;
		}

		//Calculate the angle. The output is in radians
		//This step can be skipped for optimization...
		angle = math.acos(dot);

		return (float)angle;
	}
}
