using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

public class GravityModifier : PowerUp
{
    public float timeBonus = 5.0f;
    private void Awake()
    {
        identifier = "GravityModifier";
        autoUse = true;
    }

    public override bool PickUp(Marble4D marble)
    {
        var o4d = this.gameObject.GetComponent<Object4D>();
        var upDir = new Vector4(0, -1, 0, 0);
        upDir = o4d.WorldTransform4D().matrix * upDir;
        upDir.Normalize();
        return marble.currentUp != upDir;
    }

    public override void Use(Marble4D marble, TimeState t)
    {
        var o4d = this.gameObject.GetComponent<Object4D>();
        var upDir = new Vector4(0, -1, 0, 0);
        upDir = o4d.WorldTransform4D().matrix * upDir;
        upDir.Normalize();
        marble.SetUp(upDir, t);
    }
}

