using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeFallManager
{
    private float velocity;
    private float height;
    private float gravity;
    
    public FreeFallManager(float initV, float gravity, float initH, float goalH)
    {
        velocity = initV;
        height = initH;
        this.gravity = gravity;
    }

    public void Proceed(float dt)
    {
        height += velocity * dt;
        velocity -= gravity * dt;
    }

    public float GetHeight()
    {
        return height;
    }
}
