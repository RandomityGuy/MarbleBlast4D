//#########[---------------------------]#########
//#########[  GENERATED FROM TEMPLATE  ]#########
//#########[---------------------------]#########
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderGroup4D {
    public List<Collider4D> colliders = new();
    private Vector4 aabbMin = float.MaxValue * Vector4.one;
    private Vector4 aabbMax = float.MinValue * Vector4.one;
    public Vector4 worldAabbMin = Vector4.one * float.MaxValue;
    public Vector4 worldAabbMax = Vector4.one * float.MinValue;
    public int key = -1;
    
    public void Add(Collider4D collider) {
        Debug.Assert(collider.aabbMin.x != float.MaxValue);
        Debug.Assert(collider.aabbMax.x != float.MinValue);
        colliders.Add(collider);
        aabbMin = Vector4.Min(aabbMin, collider.aabbMin);
        aabbMax = Vector4.Max(aabbMax, collider.aabbMax);
        collider.CalculateWorldAABB();
        
        Transform4D localToWorld4D = collider.gameObject.GetComponent<Object4D>().WorldTransform4D();

        var p1 = localToWorld4D * aabbMin;
        var p2 = localToWorld4D * aabbMax;

        worldAabbMin = new Vector4(Mathf.Min(p1.x, p2.x), Mathf.Min(p1.y, p2.y), Mathf.Min(p1.z, p2.z), Mathf.Min(p1.w, p2.w));
        worldAabbMax = new Vector4(Mathf.Max(p1.x, p2.x), Mathf.Max(p1.y, p2.y), Mathf.Max(p1.z, p2.z), Mathf.Max(p1.w, p2.w));
    }

    public bool IntersectsAABB(Transform4D localToWorld4D, Transform4D worldToLocal4D, Vector4 worldPt, float radius) {
        Vector4 localPt = worldToLocal4D * worldPt;
        Vector4 boundingNP = localToWorld4D * Vector4.Min(Vector4.Max(localPt, aabbMin), aabbMax);
        return Vector4.Distance(worldPt, boundingNP) <= radius;
    }

    public bool IntersectsAABB(Vector4 worldPt, float radius)
    {
        Vector4 boundingNP = Vector4.Min(Vector4.Max(worldPt, worldAabbMin), worldAabbMax);
        return Vector4.Distance(worldPt, boundingNP) <= radius;
    }
}
