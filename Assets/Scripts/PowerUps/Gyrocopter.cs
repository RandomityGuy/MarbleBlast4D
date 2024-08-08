using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

public class Gyrocopter : PowerUp
{

    private void Awake()
    {
        identifier = "Gyrocopter";
    }

    public override bool PickUp(Marble4D marble)
    {
        return world.PickUpPowerup(marble, this);
    }

    public override void Use(Marble4D marble, TimeState t)
    {
        marble.EnableHelicopter(t);
        world.DeselectPowerup(marble);
    }
}

