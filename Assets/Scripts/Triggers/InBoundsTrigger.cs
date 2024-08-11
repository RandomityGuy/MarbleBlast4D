using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class InBoundsTrigger : TriggerEntity
{
    public override void OnEnter(Marble4D marble, TimeState t)
    {
       
    }

    public override void OnLeave(Marble4D marble, TimeState t)
    {
        marble.SetOutOfBounds(true);
    }
}

