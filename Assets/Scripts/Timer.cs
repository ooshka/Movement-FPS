using UnityEngine;

public class Timer
{
    private readonly float duration;
    private readonly bool eventDuringTimer;

    private float timer;

    public Timer(float duration, bool eventDuringTimer)
    {
        this.duration = duration;
        this.eventDuringTimer = eventDuringTimer;

        timer = duration;
    }

    public void Iterate(float timePassed)
    {
        if (timer != 0)
        {
            timer = Mathf.Max(0, timer - timePassed);
        } 
    }

    public void Reset()
    {
        timer = duration;
    }

    public void AddTime(float time)
    {
        timer += time;
    }

    public bool CanTriggerEvent()
    {
        if (eventDuringTimer)
        {
            return timer > 0;
        } else
        {
            return timer == 0;
        }
    }

    public bool CanTriggerEventAndReset()
    {
        bool canTrigger = CanTriggerEvent();
        if (canTrigger)
        {
            Reset();
        }
        return canTrigger;
    }

    public float GetTime()
    {
        return timer;
    }

    public void SetTime(float time)
    {
        timer = time;
    }
}
