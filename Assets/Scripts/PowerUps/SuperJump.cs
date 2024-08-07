using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SuperJump : PowerUp
{

    private void Awake()
    {
        identifier = "SuperJump";
    }

    public override bool PickUp(Marble4D marble)
    {
        return world.PickUpPowerup(marble, this);
    }

    public override void Use(Marble4D marble, TimeState t)
    {
        marble.velocity += marble.currentUp * 20f;
        world.DeselectPowerup(marble);
    }
}

