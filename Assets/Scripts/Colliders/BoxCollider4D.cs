//#########[---------------------------]#########
//#########[  GENERATED FROM TEMPLATE  ]#########
//#########[---------------------------]#########
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxCollider4D : Collider4D {
    public Vector4 pos = Vector4.zero;
    public Vector4 size = Vector4.one;
    public Matrix4x4 basis = Matrix4x4.identity;

    protected override void Awake() {
        base.Awake();
        UpdateBoundingPoints();
    }

    public void UpdateBoundingPoints() {
        ResetBoundingBox();
        for (int i = 0; i < (1 << 4); ++i) {
            Vector4 s = size;
            for (int j = 0; j < 4; ++j) {
                if ((i & (1 << j)) != 0) { s[j] = -s[j]; }
            }
            AddBoundingPoint(pos + basis * s);
        }
        CalculateWorldAABB();
    }

    public override Vector4 NP(Vector4 localPt) {
        Vector4 p = basis.transpose * (localPt - pos);
        p = Vector4.Max(Vector4.Min(p, size), -size);
        return pos + basis * p;
    }

    private void OnDrawGizmosSelected()
    {
        if (obj4D == null)
        {
            obj4D = gameObject.GetComponent<Object4D>();
        }
        var wt = obj4D.WorldTransform4D();

        var points = new Vector4[16];

        for (int i = 0; i < (1 << 4); ++i)
        {
            Vector4 s = size;
            for (int j = 0; j < 4; ++j)
            {
                if ((i & (1 << j)) != 0) { s[j] = -s[j]; }
            }
            points[i] = wt.matrix * (pos + basis * s) + wt.translation;
        }

        Gizmos.DrawLine(points[0], points[1]);
        Gizmos.DrawLine(points[0], points[2]);
        Gizmos.DrawLine(points[1], points[3]);
        Gizmos.DrawLine(points[2], points[3]);
        Gizmos.DrawLine(points[0], points[4]);
        Gizmos.DrawLine(points[6], points[7]);
        Gizmos.DrawLine(points[4], points[5]);
        Gizmos.DrawLine(points[4], points[6]);
        Gizmos.DrawLine(points[3], points[7]);
        Gizmos.DrawLine(points[7], points[5]);
        Gizmos.DrawLine(points[5], points[1]);
        Gizmos.DrawLine(points[2], points[6]);

        //Gizmos.DrawLineList(points);

        //var p1 = (wt.matrix * basis).MultiplyPoint(new Vector3(0, 0, 0)) + Transform4D.XYZ(wt.translation) - new Vector3(size.x, size.y, size.z);
        //var p2 = (wt.matrix * basis).MultiplyPoint(new Vector3(size.x * 2, 0, 0)) + Transform4D.XYZ(wt.translation) - new Vector3(size.x, size.y, size.z);
        //var p3 = (wt.matrix * basis).MultiplyPoint(new Vector3(size.x * 2, size.y * 2, 0)) + Transform4D.XYZ(wt.translation) - new Vector3(size.x, size.y, size.z);
        //var p4 = (wt.matrix * basis).MultiplyPoint(new Vector3(0, size.y * 2, 0)) + Transform4D.XYZ(wt.translation) - new Vector3(size.x, size.y, size.z);
        //var p5 = (wt.matrix * basis).MultiplyPoint(new Vector3(0, 0, size.z * 2)) + Transform4D.XYZ(wt.translation) - new Vector3(size.x, size.y, size.z);
        //var p6 = (wt.matrix * basis).MultiplyPoint(new Vector3(size.x * 2, 0, size.z * 2)) + Transform4D.XYZ(wt.translation) - new Vector3(size.x, size.y, size.z);
        //var p7 = (wt.matrix * basis).MultiplyPoint(new Vector3(size.x * 2, size.y * 2, size.z * 2)) + Transform4D.XYZ(wt.translation) - new Vector3(size.x, size.y, size.z);
        //var p8 = (wt.matrix * basis).MultiplyPoint(new Vector3(0, size.y * 2, size.z * 2)) + Transform4D.XYZ(wt.translation) - new Vector3(size.x, size.y, size.z);
        //Gizmos.DrawLine(p1, p2);
        //Gizmos.DrawLine(p1, p4);
        //Gizmos.DrawLine(p1, p5);
        //Gizmos.DrawLine(p7, p8);
        //Gizmos.DrawLine(p7, p6);
        //Gizmos.DrawLine(p7, p3);
        //Gizmos.DrawLine(p2, p3);
        //Gizmos.DrawLine(p2, p6);
        //Gizmos.DrawLine(p5, p8);
        //Gizmos.DrawLine(p3, p4);
        //Gizmos.DrawLine(p5, p6);
        //Gizmos.DrawLine(p4, p8);
    }
}