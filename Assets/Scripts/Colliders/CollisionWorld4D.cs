﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class CollisionWorld4D
{
    GridBroadphase4D grid;
    List<Collider4D> entities;

    public CollisionWorld4D()
    {
        grid = new GridBroadphase4D();
        entities = new List<Collider4D>();
    }

    public void AddEntity(Collider4D collider)
    {
        entities.Add(collider);
        grid.Insert(collider);
    }

    public void RemoveEntity(Collider4D collider)
    {
        entities.Remove(collider);
        grid.Remove(collider);
    }

    public void UpdateEntity(Collider4D collider)
    {
        grid.Update(collider);
    }

    public List<Collider4D> SphereIntersection(Vector4 pos, float radius)
    {
        Vector4 min = pos - new Vector4(radius, radius, radius, radius);
        Vector4 max = pos + new Vector4(radius, radius, radius, radius);
        return grid.BoundingSearch(min, max);
    }
}

