using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class TriggerEntity : Trigger
{
    public abstract void OnEnter(Marble4D marble, TimeState t);
    public abstract void OnLeave(Marble4D marble, TimeState t);
}

