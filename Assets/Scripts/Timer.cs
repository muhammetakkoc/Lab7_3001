using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    private float timeRemaining;
    private float duration;
    private bool running;

    public Timer(float duration)
    {
        this.duration = duration;
        this.timeRemaining = duration;
        this.running = false;
    }

    public Timer() : this(0.0f) { }

    public void Tick(float deltaTime)
    {
        if (running)
        {
            timeRemaining -= deltaTime;
            if (timeRemaining <= 0)
            {
                running = false;
                timeRemaining = 0;
            }
        }
    }

    public void Reset()
    {
        timeRemaining = duration;
        running = true;
    }

    public void Reset(float newDuration)
    {
        duration = newDuration;
        Reset();
    }

    public bool Expired()
    {
        return !running;
    }
}
