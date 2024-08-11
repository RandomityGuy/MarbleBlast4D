using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemItem : Trigger
{
    Object4D obj4D;
    public bool hidden = false;
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

    public void SetHidden(bool hidden)
    {
        this.gameObject.GetComponent<MeshRenderer>().enabled = !hidden;
        this.hidden = hidden;
    }

    public override void OnCollide(Marble4D marble, TimeState t)
    {
        if (hidden) return;
        SetHidden(true);
        world.PickUpGem(this);
    }

    public override void Reset()
    {
        SetHidden(false);
    }
}
