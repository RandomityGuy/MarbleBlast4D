using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

public class TimeTravel : PowerUp
{
    public float timeBonus = 5.0f;
    private void Awake()
    {
        identifier = "TimeTravel";
        autoUse = true;
    }

    public override bool PickUp(Marble4D marble)
    {
        return true;
    }

    public override void Use(Marble4D marble, TimeState t)
    {
        world.AddBonusTime(timeBonus);
    }
}

