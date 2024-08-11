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
    public float dt;
}

[DefaultExecutionOrder(-99)]
public class MarbleWorld4D : MonoBehaviour
{
    [System.NonSerialized] public CollisionWorld4D collisionWorld4D;
    Marble4D marble;
    StartPad startPad;

    GemItem[] gems;
    PowerUp[] powerUps;
    TriggerEntity[] triggers;
    int collectedGems = 0;

    TimerComponent timer;
    GemCounter gemCounter;
    [NonSerialized] public TimeState timeState;
    float bonusTime = 0;
    TimeState? finishTime;

    float fixedDeltaTimeAccumulator = 0.0f;
    
    private void Awake()
    {
        collisionWorld4D = new CollisionWorld4D();
        timer = FindFirstObjectByType<TimerComponent>();
        gemCounter = FindFirstObjectByType<GemCounter>();
        marble = FindFirstObjectByType<Marble4D>();
        marble.world = this;
        startPad = FindFirstObjectByType<StartPad>();
        timeState = new TimeState();
        timeState.currentAttemptTime = 0;
        timeState.totalAttemptTime = 0;
        timeState.gameplayClock = 0;
        finishTime = null;

        gems = FindObjectsOfType<GemItem>();
        foreach (var gem in gems)
            gem.world = this;
        powerUps = FindObjectsOfType<PowerUp>();
        foreach (var powerUp in powerUps)
            powerUp.world = this;
        triggers = FindObjectsOfType<TriggerEntity>();
        foreach (var trigger in triggers)
            trigger.world = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        var colliders = Collider4D.MakeColliderGroups(FindObjectsOfType<Collider4D>());
        foreach (var col in colliders)
        {
            collisionWorld4D.AddEntity(col.Value);
        }
        collisionWorld4D.Build();
        Restart();
    }

    void Restart()
    {
        marble.SetPosition(startPad.GetComponent<Object4D>().worldPosition4D + new Vector4(0, 3, 0, 0));
        marble.velocity.Set(0, 0, 0, 0);
        marble.omega = BiVector3.zero;

        fixedDeltaTimeAccumulator = 0;
        timeState.gameplayClock = 0;
        timeState.currentAttemptTime = 0;
        bonusTime = 0;
        timer.SetTime(timeState.gameplayClock);
        collectedGems = 0;

        foreach (var gem in gems)
            gem.SetHidden(false);

        gemCounter.SetGemCount(0, gems.Length);
    }

    void UpdateTimer()
    {
        timeState.dt = Time.deltaTime;
        timeState.currentAttemptTime += Time.deltaTime;
        timeState.totalAttemptTime += Time.deltaTime;

        if (this.bonusTime != 0 && this.timeState.currentAttemptTime >= 3.5)
        {
            this.bonusTime -= Time.deltaTime;
            if (this.bonusTime < 0)
            {
                this.timeState.gameplayClock -= this.bonusTime;
                this.bonusTime = 0;
            }
            //if (timeTravelSound == null)
            //{
            //    var ttsnd = ResourceLoader.getResource("data/sound/timetravelactive.wav", ResourceLoader.getAudio, this.soundResources);
            //    timeTravelSound = AudioManager.playSound(ttsnd, null, true);

            //    if (alarmSound != null)
            //        alarmSound.pause = true;
            //}
        } 
        else
        {
            if (timeState.currentAttemptTime >= 3.5f)
            {
                timeState.gameplayClock += Time.deltaTime;
            }
            else if (timeState.currentAttemptTime + Time.deltaTime >= 3.5f)
            {
                timeState.gameplayClock += (this.timeState.currentAttemptTime + Time.deltaTime) - 3.5f;
            }
        }

        if (finishTime != null)
        {
            timeState.gameplayClock = finishTime.Value.gameplayClock;
        }
        timer.SetTime(timeState.gameplayClock);
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

    public void PickUpGem(GemItem gem)
    {
        collectedGems += 1;
        gemCounter.SetGemCount(collectedGems, gems.Length);
    }

    private void Update()
    {
        UpdateTimer();
        UpdateGameState();

        fixedDeltaTimeAccumulator += Time.deltaTime;

        while (fixedDeltaTimeAccumulator >= Time.fixedDeltaTime)
        {
            var ft = timeState;
            ft.dt = Time.fixedDeltaTime;
            marble.UpdateFixedMB(ft);
            fixedDeltaTimeAccumulator -= Time.fixedDeltaTime;
        }

        marble.UpdateMB(timeState);
        foreach (var powerUp in powerUps)
            powerUp.UpdateMB(timeState);
    }

    public bool PickUpPowerup(Marble4D marble, PowerUp powerup)
    {
        if (powerup == null)
            return false;
        if (marble.heldPowerup != null && marble.heldPowerup.identifier == powerup.identifier)
            return false;
        marble.heldPowerup = powerup;
        return true;
    }

    public void DeselectPowerup(Marble4D marble)
    {
        marble.heldPowerup = null;
    }

    public void AddBonusTime(float time)
    {
        bonusTime += time;
    }
}

