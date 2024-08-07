using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PowerUp : Trigger
{
    Object4D obj4D;
    public bool hidden = false;

    public float lastPickupTime = -1f;
    public float cooldownDuration = 7f;
    public bool autoUse = false;
    public string pickUpName;
    public string identifier;

    // Start is called before the first frame update
    void Start()
    {
        obj4D = gameObject.GetComponent<Object4D>();
    }

    // Update is called once per frame
    void Update()
    {
        var rot = Time.timeSinceLevelLoad * (-1 / 3.0f) * 360.0f;
        obj4D.localRotation4D = Transform4D.PlaneRotation(rot, 0, 2);
    }

    public virtual void UpdateMB(TimeState timeState)
    {
        var opacity = 1.0;
        if (this.lastPickupTime > 0 && this.cooldownDuration > 0)
        {
            var availableTime = this.lastPickupTime + this.cooldownDuration;
            opacity = Mathf.Clamp((timeState.currentAttemptTime - availableTime), 0, 1);
        }
        if (hidden && opacity == 1)
        {
            hidden = false;
            SetHidden(false);
        }
        if (!hidden && opacity < 0)
        {
            hidden = true;
            SetHidden(true);
        }
    }

    public void SetHidden(bool hidden)
    {
        this.gameObject.GetComponent<MeshRenderer>().enabled = !hidden;
        this.hidden = hidden;
    }

    public override void OnCollide(Marble4D marble, TimeState t)
    {
        var pickupable = lastPickupTime == -1f || (t.currentAttemptTime - lastPickupTime) >= cooldownDuration;
        if (!pickupable)
            return;
        if (PickUp(marble))
        {
            lastPickupTime = t.currentAttemptTime;
            SetHidden(true);
            if (autoUse)
                Use(marble, t);
        }
    }

    public abstract bool PickUp(Marble4D marble);

    public abstract void Use(Marble4D marble, TimeState t);
}
