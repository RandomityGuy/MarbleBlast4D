using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class GridBroadphaseProxy
{
    public int index;
    public Collider4D collider;
    public int xMin;
    public int xMax;
    public int yMin;
    public int yMax;
}

public class GridBroadphase4D
{
    Vector4 boundsMin;
    Vector4 boundsMax;

    static int CELL_SIZE = 8;
    static Vector2 CELL_DIV = new Vector3(CELL_SIZE, CELL_SIZE);

    Vector2 cellSize;

    List<int>[,] cells = new List<int>[CELL_SIZE, CELL_SIZE];

    List<GridBroadphaseProxy> objects = new();
    Dictionary<Collider4D, GridBroadphaseProxy> objectToProxy = new();

    int searchKey = 0;
    bool built = false;

    public GridBroadphase4D()
    {
        for (int i = 0; i < CELL_SIZE; i++)
        {
            for (int j = 0; j < CELL_SIZE; j++)
            {
                cells[i, j] = new();
            }
        }
    }

    public void Insert(Collider4D collider)
    {
        if (!built)
        {
            int idx = objects.Count;
            objects.Add(new GridBroadphaseProxy
            {
                collider = collider,
                xMin = 1000,
                yMin = 1000,
                xMax = -1000,
                yMax = -1000,
                index = idx,
            });
            objectToProxy[collider] = objects[objects.Count - 1];
        }
        else
        {
            int idx = objects.Count;
            GridBroadphaseProxy proxy = new GridBroadphaseProxy
            {
                collider = collider,
                xMin = 1000,
                yMin = 1000,
                xMax = -1000,
                yMax = -1000,
                index = idx,
            };
            objects.Add(proxy);
            objectToProxy[collider] = proxy;

            Transform4D localToWorld4D = collider.obj4D.WorldTransform4D();

            Vector4 queryMin = Vector4.Max(localToWorld4D * collider.aabbMin, boundsMin);
            Vector4 queryMax = Vector4.Min(localToWorld4D * collider.aabbMax, boundsMax);
            Vector2Int start = new Vector2Int(
                (int)((queryMin.x - boundsMin.x) / cellSize.x),
                (int)((queryMin.y - boundsMin.y) / cellSize.y));
            Vector2Int end = new Vector2Int(
                (int)((queryMax.x - boundsMin.x) / cellSize.x),
                (int)((queryMax.y - boundsMin.y) / cellSize.y));

            for (int i = start.x; i <= end.x; i++)
            {
                for (int j = start.y; j <= end.y; j++)
                {
                    cells[i, j].Add(idx);
                    proxy.xMin = Math.Min(proxy.xMin, i);
                    proxy.yMin = Math.Min(proxy.yMin, j);
                    proxy.xMax = Math.Max(proxy.xMax, i);
                    proxy.yMax = Math.Max(proxy.yMax, j);
                }
            }
        }
    }

    public void Remove(Collider4D obj)
    {
        if (!objectToProxy.ContainsKey(obj))
            return;

        GridBroadphaseProxy proxy = objectToProxy[obj];
        for (int i = proxy.xMin; i <= proxy.xMax + 1; i++)
        {
            for (int j = proxy.yMin; j <= proxy.yMax + 1; j++)
            {
                cells[i, j].Remove(proxy.index);
            }
        }
        objects[proxy.index] = null;
        objectToProxy.Remove(obj);
    }
    
    public void Update(Collider4D obj)
    {
        if (!built)
            return;

        Transform4D localToWorld4D = obj.obj4D.WorldTransform4D();
        Vector4 aabbMin = localToWorld4D * obj.aabbMin;
        Vector4 aabbMax = localToWorld4D * obj.aabbMax;

        obj.CalculateWorldAABB();

        var queryMin = new Vector2(Math.Max(aabbMin.x, boundsMin.x), Math.Max(aabbMin.y, boundsMin.y));
        var queryMax = new Vector2(Math.Min(aabbMax.x, boundsMax.x), Math.Min(aabbMax.y, boundsMax.y));
        var start = new Vector2Int((int)Math.Floor((queryMax.x - boundsMin.x) / cellSize.x), (int)Math.Floor((queryMin.y - boundsMin.y) / cellSize.y));
        var end = new Vector2Int((int)Math.Floor((queryMax.x - boundsMin.x) / cellSize.x), (int)Math.Floor((queryMax.y - boundsMin.y) / cellSize.y));
        var proxy = objectToProxy.GetValueOrDefault(obj, null);
        if (proxy == null)
        {
            Insert(obj);
        } else {
            if (start.x != proxy.xMin || start.y != proxy.yMin || end.x != proxy.xMax || end.y != proxy.yMax)
            {
                // Rebin the object
                for (var i = proxy.xMin; i < proxy.xMax + 1; i++)
                {
                    for (var j = proxy.yMin; j < proxy.yMax + 1; j++)
                    {
                        cells[i, j].Remove(proxy.index);
                    }
                }

                for (var i = start.x; i < end.x + 1; i++)
                {
                    for (var j = start.y; j < end.y + 1; j++)
                    {
                        cells[i, j].Remove(proxy.index);
                    }
                }

                proxy.xMin = start.x;
                proxy.yMin = start.y;
                proxy.xMax = end.x;
                proxy.yMax = end.y;
            }

        }
    }

    public void Build()
    {
        if (built)
            return;
        built = true;
        // Find the bounds
        var xMin = 1e8f;
        var xMax = -1e8f;
        var yMin = 1e8f;
        var yMax = -1e8f;
        var zMin = 1e8f;
        var zMax = -1e8f;


        for (var i = 0; i < this.objects.Count; i++)
        {
            if (this.objects[i] == null)
                continue;
            var surface = this.objects[i].collider;

            Transform4D localToWorld4D = surface.obj4D.WorldTransform4D();
            Vector4 aabbMin = localToWorld4D * surface.aabbMin;
            Vector4 aabbMax = localToWorld4D * surface.aabbMax;

            surface.CalculateWorldAABB();

            xMin = Math.Min(xMin, aabbMin.x);
            xMax = Math.Max(xMax, aabbMax.x);
            yMin = Math.Min(yMin, aabbMin.y);
            yMax = Math.Max(yMax, aabbMax.y);
            zMin = Math.Min(zMin, aabbMin.z);
            zMax = Math.Max(zMax, aabbMax.z);
        }
        // Some padding
        xMin -= 100;
        xMax += 100;
        yMin -= 100;
        yMax += 100;
        zMin -= 100;
        zMax += 100;
        this.boundsMin = new Vector4(xMin, yMin, zMin);
        this.boundsMax = new Vector4(xMax, yMax, zMax);
        this.cellSize = new Vector2((xMax - xMin) / CELL_DIV.x, (yMax - yMin) / CELL_DIV.y);

        // Insert the objects
        for (var i = 0; i < CELL_SIZE; i++)
        {
            var minX = this.boundsMin.x;
            var maxX = this.boundsMax.x;
            minX += i * this.cellSize.x;
            maxX += (i + 1) * this.cellSize.x;
            for (var j = 0; j < CELL_SIZE; j++)
            {
                var minY = this.boundsMin.y;
                var maxY = this.boundsMax.y;
                minY += j * this.cellSize.y;
                maxY += (j + 1) * this.cellSize.y;

                var binRect = new Rect(minX, minY, maxX - minX, maxY - minY);

                for (var idx = 0; idx < objects.Count; idx++)
                {
                    if (this.objects[idx] == null)
                        continue;
                    var surface = this.objects[idx];

                    Transform4D localToWorld4D = surface.collider.obj4D.WorldTransform4D();
                    Vector4 aabbMin = localToWorld4D * surface.collider.aabbMin;
                    Vector4 aabbMax = localToWorld4D * surface.collider.aabbMax;

                    var hullRect = new Rect(aabbMin.x, aabbMin.y, aabbMax.x - aabbMin.x, aabbMax.y - aabbMin.y);
                    
                    if (hullRect.Overlaps(binRect))
                    {
                        this.cells[i,j].Add(idx);
                        surface.xMin = Math.Min(surface.xMin, i);
                        surface.yMin = Math.Min(surface.yMin, j);
                        surface.xMax = Math.Max(surface.xMax, i);
                        surface.yMax = Math.Max(surface.yMax, j);
                    }
                }
            }
        }
    }

    public List<Collider4D> BoundingSearch(Vector4 aabbMin, Vector4 aabbMax)
    {
        var queryMinX = Math.Max(aabbMin.x,boundsMin.x);
        var queryMinY = Math.Max(aabbMin.y,boundsMin.y);
        var queryMaxX = Math.Min(aabbMax.x,boundsMax.x);
        var queryMaxY = Math.Min(aabbMax.y,boundsMax.y);
        int xStart = (int)Math.Floor((queryMinX - boundsMin.x) / this.cellSize.x);
        int yStart = (int)Math.Floor((queryMinY - boundsMin.y) / this.cellSize.y);
        int xEnd = (int)Math.Ceiling((queryMaxX - boundsMin.x) / this.cellSize.x);
        int yEnd = (int)Math.Ceiling((queryMaxY - boundsMin.y) / this.cellSize.y);

        if (xStart < 0)
            xStart = 0;
        if (yStart < 0)
            yStart = 0;
        if (xEnd > CELL_SIZE)
            xEnd = CELL_SIZE;
        if (yEnd > CELL_SIZE)
            yEnd = CELL_SIZE;

        var foundSurfaces = new List<Collider4D>();

        searchKey++;

        // Insert the surface references from [xStart, yStart, zStart] to [xEnd, yEnd, zEnd] into the map
        for (var i = xStart; i < xEnd; i++)
        {
            for (var j = yStart; j < yEnd; j++)
            {
                foreach (var surfIdx in cells[i, j])
                {
                    var surf = objects[surfIdx].collider;
                    if (surf.key == searchKey)
                        continue;
                    surf.key = searchKey;
                    if (containsBounds(aabbMin, aabbMax, surf.worldAabbMin, surf.worldAabbMax) || collide(aabbMin, aabbMax, surf.worldAabbMin, surf.worldAabbMax))
                    {
                        foundSurfaces.Add(surf);
                        surf.key = searchKey;
                    }
                }
            }
        }

        return foundSurfaces;
    }


    bool collide(Vector4 aMin, Vector4 aMax, Vector4 bMin, Vector4 bMax)
    {
        return !(aMin.x > bMax.x || aMin.y > bMax.y || aMin.z > bMax.z || aMin.w > bMax.w || aMax.x < bMin.x || aMax.y < bMin.y || aMax.z < bMin.z || aMax.w < bMin.w);
    }

    bool containsBounds(Vector4 aMin, Vector4 aMax, Vector4 bMin, Vector4 bMax)
    {
        return aMin.x <= bMin.x && aMin.y <= bMin.y && aMin.z <= bMin.z && aMin.w <= bMin.w && aMax.x >= bMax.x && aMax.y >= bMax.y && aMax.z >= bMax.z && aMax.w >= bMax.w;
    }
}

