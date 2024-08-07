using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

public class SuperSpeed : PowerUp
{

    private void Awake()
    {
        identifier = "SuperSpeed";
    }

    public override bool PickUp(Marble4D marble)
    {
        return world.PickUpPowerup(marble, this);
    }

    public override void Use(Marble4D marble, TimeState t)
    {
        marble._getMarbleAxis(out Vector4 sideDir, out Vector4 movementDir, out Vector4 upDir, out Vector4 wDir);
        var boostVector = movementDir;
        var contactDot = Vector4.Dot(movementDir, marble.lastContactNormal);
        boostVector -= marble.lastContactNormal * contactDot;
        marble.velocity += boostVector * 25f;
        world.DeselectPowerup(marble);
    }
}

