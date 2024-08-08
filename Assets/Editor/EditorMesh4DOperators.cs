using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public enum Axis
{
    X,
    Y,
    Z,
    W
}

[Serializable]
public abstract class EditorMesh4DOperator
{
    public bool Enabled;
    public string Title;
    public float Height;
    public abstract void Draw(Rect r);
    public abstract void Apply(Mesh4DBuilder m);
}

[Serializable]
public class OperatorSmoothen4D : EditorMesh4DOperator
{
    public OperatorSmoothen4D()
    {
        Title = "Smoothen";
        Height = 20;
    }

    public override void Apply(Mesh4DBuilder m)
    {
        m.Smoothen();
    }

    public override void Draw(Rect r)
    {
    }
}

[Serializable]
public class OperatorPerturb4D : EditorMesh4DOperator
{
    public Vector4 scale;
    public OperatorPerturb4D()
    {
        Title = "Perturb";
        Height = 60;
    }

    public override void Apply(Mesh4DBuilder m)
    {
        m.Perturb(scale.x, scale.y, scale.z, scale.w);
    }

    public override void Draw(Rect r)
    {
        scale = EditorGUI.Vector4Field(r, "Scale", scale);
    }
}

[Serializable]
public class OperatorWave4D : EditorMesh4DOperator
{
    public Vector4 freq;
    public Vector4 dir;
    public OperatorWave4D()
    {
        Title = "Wave";
        Height = 100;
    }

    public override void Apply(Mesh4DBuilder m)
    {
        m.Wave(freq, dir);
    }

    public override void Draw(Rect r)
    {
        freq = EditorGUI.Vector4Field(r, "Frequency", freq);
        r.y += 35;
        dir = EditorGUI.Vector4Field(r, "Direction", dir);
    }
}

[Serializable]
public class OperatorTwist4D : EditorMesh4DOperator
{
    public Axis axis1;
    public Axis axis2;
    public float angle;
    public Axis interpAxis;
    public Vector4 pivot;
    public OperatorTwist4D()
    {
        Title = "Twist";
        Height = 150;
    }

    public override void Apply(Mesh4DBuilder m)
    {
        m.Twist(angle, (int)axis1, (int)axis2, pivot, (int)interpAxis);
    }

    public override void Draw(Rect r)
    {
        axis1 = (Axis)EditorGUI.EnumPopup(r, "From", axis1);
        r.y += 20;
        axis2 = (Axis)EditorGUI.EnumPopup(r, "To", axis2);
        r.y += 20;
        Rect r2 = r;
        r2.height = 20;
        angle = EditorGUI.FloatField(r2, "Angle", angle);
        r.y += 20;
        interpAxis = (Axis)EditorGUI.EnumPopup(r, "Interpolation Axis", interpAxis);
        r.y += 20;
        pivot = EditorGUI.Vector4Field(r, "Pivot", pivot);
    }
}

[Serializable]
public class OperatorFluff4D : EditorMesh4DOperator
{
    public float scale;
    public OperatorFluff4D()
    {
        Title = "Fluff";
        Height = 40;
    }

    public override void Apply(Mesh4DBuilder m)
    {
        m.Fluff(scale);
    }

    public override void Draw(Rect r)
    {
        scale = EditorGUI.FloatField(r, "Scale", scale);
    }
}


[Serializable]
public class OperatorMergeVerts4D : EditorMesh4DOperator
{
    public float epsilon;
    public OperatorMergeVerts4D()
    {
        Title = "Merge Vertices";
        Height = 40;
    }

    public override void Apply(Mesh4DBuilder m)
    {
        m.MergeVerts(epsilon);
    }

    public override void Draw(Rect r)
    {
        epsilon = EditorGUI.FloatField(r, "Epsilon", epsilon);
    }
}

[Serializable]
public class OperatorSpike4D : EditorMesh4DOperator
{
    public float multiplier;
    public OperatorSpike4D()
    {
        Title = "Spike";
        Height = 40;
    }

    public override void Apply(Mesh4DBuilder m)
    {
        m.Spike(multiplier);
    }

    public override void Draw(Rect r)
    {
        multiplier = EditorGUI.FloatField(r, "Multiplier", multiplier);
    }
}

[Serializable]
public class OperatorGeopoke4D : EditorMesh4DOperator
{
    public bool normalize = true;
    public bool extraPoke = false;
    public OperatorGeopoke4D()
    {
        Title = "Geopoke";
        Height = 60;
    }

    public override void Apply(Mesh4DBuilder m)
    {
        m.GeoPoke(normalize, extraPoke);
    }

    public override void Draw(Rect r)
    {
        normalize = EditorGUI.Toggle(r, "Normalize", normalize);
        r.y += 20;
        extraPoke = EditorGUI.Toggle(r, "Extra Poke", extraPoke);
    }
}

[Serializable]
public class OperatorSpherize4D : EditorMesh4DOperator
{
    public float ratio;
    public float aboveW = -99999.0f;
    public OperatorSpherize4D()
    {
        Title = "Spherize";
        Height = 60;
    }

    public override void Apply(Mesh4DBuilder m)
    {
        m.Spherize(ratio, aboveW);
    }

    public override void Draw(Rect r)
    {
        ratio = EditorGUI.FloatField(r, "Ratio", ratio);
        r.y += 20;
        aboveW = EditorGUI.FloatField(r, "Above W", aboveW);
    }
}
