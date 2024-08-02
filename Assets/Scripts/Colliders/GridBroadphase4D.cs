using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class GridBroadphaseProxy4D
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

    static int CELL_SIZE = 16;
    static Vector2 CELL_DIV = new Vector3(CELL_SIZE, CELL_SIZE);

    Vector2 cellSize;

    List<int>[,] cells = new List<int>[CELL_SIZE, CELL_SIZE];

    List<GridBroadphaseProxy4D> objects = new();
    Dictionary<Collider4D, GridBroadphaseProxy4D> objectToProxy = new();

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
            objects.Add(new GridBroadphaseProxy4D
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
            var proxy = new GridBroadphaseProxy4D
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
                (int)((queryMin.z - boundsMin.z) / cellSize.y));
            Vector2Int end = new Vector2Int(
                (int)((queryMax.x - boundsMin.x) / cellSize.x),
                (int)((queryMax.z - boundsMin.z) / cellSize.y));

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

        var proxy = objectToProxy[obj];
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

        var queryMin = new Vector2(Math.Max(aabbMin.x, boundsMin.x), Math.Max(aabbMin.z, boundsMin.z));
        var queryMax = new Vector2(Math.Min(aabbMax.x, boundsMax.x), Math.Min(aabbMax.z, boundsMax.z));
        var start = new Vector2Int((int)Math.Floor((queryMax.x - boundsMin.x) / cellSize.x), (int)Math.Floor((queryMin.y - boundsMin.z) / cellSize.y));
        var end = new Vector2Int((int)Math.Floor((queryMax.x - boundsMin.x) / cellSize.x), (int)Math.Floor((queryMax.y - boundsMin.z) / cellSize.y));
        var proxy = objectToProxy.GetValueOrDefault(obj, null);
        if (proxy == null)
        {
            Insert(obj);
        } 
        else 
        {
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
        var wMin = 1e8f;
        var wMax = -1e8f;


        for (var i = 0; i < this.objects.Count; i++)
        {
            if (this.objects[i] == null)
                continue;
            var surface = this.objects[i].collider;

            surface.CalculateWorldAABB();
            Vector4 aabbMin = surface.worldAabbMin;
            Vector4 aabbMax = surface.worldAabbMax;

            xMin = Math.Min(xMin, aabbMin.x);
            xMax = Math.Max(xMax, aabbMax.x);
            yMin = Math.Min(yMin, aabbMin.y);
            yMax = Math.Max(yMax, aabbMax.y);
            zMin = Math.Min(zMin, aabbMin.z);
            zMax = Math.Max(zMax, aabbMax.z);
            wMin = Math.Min(wMin, aabbMin.w);
            wMax = Math.Max(wMax, aabbMax.w);
        }
        // Some padding
        xMin -= 100;
        xMax += 100;
        yMin -= 100;
        yMax += 100;
        zMin -= 100;
        zMax += 100;
        wMin -= 100;
        wMax += 100;
        this.boundsMin = new Vector4(xMin, yMin, zMin, wMin);
        this.boundsMax = new Vector4(xMax, yMax, zMax, wMax);
        this.cellSize = new Vector2((xMax - xMin) / CELL_DIV.x, (zMax - zMin) / CELL_DIV.y);

        // Insert the objects
        for (var i = 0; i < CELL_SIZE; i++)
        {
            var minX = this.boundsMin.x;
            var maxX = this.boundsMax.x;
            minX += i * this.cellSize.x;
            maxX += (i + 1) * this.cellSize.x;
            for (var j = 0; j < CELL_SIZE; j++)
            {
                var minY = this.boundsMin.z;
                var maxY = this.boundsMax.z;
                minY += j * this.cellSize.y;
                maxY += (j + 1) * this.cellSize.y;

                var binRect = new Rect(minX, minY, maxX - minX, maxY - minY);

                for (var idx = 0; idx < objects.Count; idx++)
                {
                    if (this.objects[idx] == null)
                        continue;
                    var surface = this.objects[idx];

                    Vector4 aabbMin = surface.collider.worldAabbMin;
                    Vector4 aabbMax = surface.collider.worldAabbMax;

                    var hullRect = new Rect(aabbMin.x, aabbMin.z, aabbMax.x - aabbMin.x, aabbMax.z - aabbMin.z);
                    
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
        var queryMinY = Math.Max(aabbMin.z,boundsMin.z);
        var queryMaxX = Math.Min(aabbMax.x,boundsMax.x);
        var queryMaxY = Math.Min(aabbMax.z,boundsMax.z);
        int xStart = (int)Math.Floor((queryMinX - boundsMin.x) / this.cellSize.x);
        int yStart = (int)Math.Floor((queryMinY - boundsMin.z) / this.cellSize.y);
        int xEnd = (int)Math.Ceiling((queryMaxX - boundsMin.x) / this.cellSize.x);
        int yEnd = (int)Math.Ceiling((queryMaxY - boundsMin.z) / this.cellSize.y);

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

