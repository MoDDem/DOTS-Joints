using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

internal class MeshBender
{
    private bool isDirty = false;
    private Mesh result;
    private bool useSpline;
    private Spline spline;
    private float intervalStart, intervalEnd;
    private CubicBezierCurve curve;
    private Dictionary<float, CurveSample> sampleCache = new Dictionary<float, CurveSample>();

    private SourceMesh source;

    public void SetInterval(CubicBezierCurve curve)
    {
        if (this.curve == curve) return;
        if (curve == null) throw new ArgumentNullException("curve");
        if (this.curve != null)
        {
            this.curve.Changed.RemoveListener(SetDirty);
        }
        this.curve = curve;
        spline = null;
        curve.Changed.AddListener(SetDirty);
        useSpline = false;
        SetDirty();
    }

    public void SetInterval(Spline spline, float intervalStart, float intervalEnd = 0)
    {
        if (this.spline == spline && this.intervalStart == intervalStart && this.intervalEnd == intervalEnd) return;
        if (spline == null) throw new ArgumentNullException("spline");
        if (intervalStart < 0 || intervalStart >= spline.Length)
        {
            throw new ArgumentOutOfRangeException("interval start must be 0 or greater and lesser than spline length (was " + intervalStart + ")");
        }
        if (intervalEnd != 0 && intervalEnd <= intervalStart || intervalEnd > spline.Length)
        {
            throw new ArgumentOutOfRangeException("interval end must be 0 or greater than interval start, and lesser than spline length (was " + intervalEnd + ")");
        }
        if (this.spline != null)
        {
            // unlistening previous spline
            this.spline.CurveChanged.RemoveListener(SetDirty);
        }
        this.spline = spline;
        // listening new spline
        spline.CurveChanged.AddListener(SetDirty);

        curve = null;
        this.intervalStart = intervalStart;
        this.intervalEnd = intervalEnd;
        useSpline = true;
        SetDirty();
    }

    private void SetDirty() => isDirty = true;

    public void ComputeIfNeeded()
    {
        if (isDirty)
        {
            Compute();
        }
    }

    private void Compute()
    {
        isDirty = false;
        FillStretch();
        /*
        switch (Mode)
        {
            case FillingMode.Once:
                FillOnce();
                break;
            case FillingMode.Repeat:
                FillRepeat();
                break;
            case FillingMode.StretchToInterval:
                FillStretch();
                break;
        }*/
    }

    private void FillStretch()
    {
        var bentVertices = new List<MeshVertex>(source.Vertices.Count);
        sampleCache.Clear();
        // for each mesh vertex, we found its projection on the curve
        foreach (var vert in source.Vertices)
        {
            float distanceRate = source.Length == 0 ? 0 : Math.Abs(vert.position.x - source.MinX) / source.Length;
            CurveSample sample;
            if (!sampleCache.TryGetValue(distanceRate, out sample))
            {
                if (!useSpline)
                {
                    sample = curve.GetSampleAtDistance(curve.Length * distanceRate);
                }
                else
                {
                    float intervalLength = intervalEnd == 0 ? spline.Length - intervalStart : intervalEnd - intervalStart;
                    float distOnSpline = intervalStart + intervalLength * distanceRate;
                    if (distOnSpline > spline.Length)
                    {
                        distOnSpline = spline.Length;
                        Debug.Log("dist " + distOnSpline + " spline length " + spline.Length + " start " + intervalStart);
                    }

                    sample = spline.GetSampleAtDistance(distOnSpline);
                }
                sampleCache[distanceRate] = sample;
            }

            bentVertices.Add(sample.GetBent(vert));
        }

        MeshUtility.Update(result,
            source.Mesh,
            source.Triangles,
            bentVertices.Select(b => b.position).Cast<Vector3>(),
            bentVertices.Select(b => b.normal).Cast<Vector3>());
        /*
        if (TryGetComponent(out MeshCollider collider))
        {
            collider.sharedMesh = result;
        }*/
    }
}
