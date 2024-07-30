using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct TimeState
{
    public float currentAttemptTime;
    public float totalAttemptTime;
    public float gameplayClock;
}

[DefaultExecutionOrder(-99)]
public class MarbleWorld4D : MonoBehaviour
{
    [System.NonSerialized] public CollisionWorld4D collisionWorld4D;
    Marble4D marble;
    StartPad startPad;

    [NonSerialized] public TimeState timeState;
    TimeState? finishTime;
    
    private void Awake()
    {
        collisionWorld4D = new CollisionWorld4D();
        marble = FindFirstObjectByType<Marble4D>();
        marble.world = this;
        startPad = FindFirstObjectByType<StartPad>();
        var colliders = FindObjectsOfType<Collider4D>();
        foreach (var col in colliders)
        {
            collisionWorld4D.AddEntity(col);
        }
        timeState = new TimeState();
        timeState.currentAttemptTime = 0;
        timeState.totalAttemptTime = 0;
        timeState.gameplayClock = 0;
        finishTime = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        collisionWorld4D.Build();
        Restart();
    }

    void Restart()
    {
        marble.SetPosition(startPad.GetComponent<Object4D>().worldPosition4D + new Vector4(0, 3, 0, 0));
        marble.velocity.Set(0, 0, 0, 0);
        marble.omega = BiVector3.zero;
        
        timeState.gameplayClock = 0;
        timeState.currentAttemptTime = 0;
    }

    void UpdateTimer()
    {
        timeState.currentAttemptTime += Time.fixedDeltaTime;
        timeState.totalAttemptTime += Time.fixedDeltaTime;
        if (timeState.currentAttemptTime >= 3.5f)
        {
            timeState.gameplayClock += Time.fixedDeltaTime;
        } 
        else if (timeState.currentAttemptTime + Time.fixedDeltaTime >= 3.5f)
        {
            timeState.gameplayClock += (this.timeState.currentAttemptTime + Time.fixedDeltaTime) - 3.5f;
        }
        if (finishTime != null)
        {
            timeState.gameplayClock = finishTime.Value.gameplayClock;
        }
    }

    void UpdateGameState()
    {
        if (timeState.currentAttemptTime < 3.5 && finishTime == null)
            marble.mode = MarbleMode.Start;
        else if (timeState.currentAttemptTime >= 3.5 && finishTime == null)
            marble.mode = MarbleMode.Normal;
    }

    public void TouchFinish()
    {
        if (finishTime != null) return;
        marble.mode = MarbleMode.Finish;
        finishTime = timeState;
    }

    private void FixedUpdate()
    {
        UpdateTimer();
        UpdateGameState();
    }
}

