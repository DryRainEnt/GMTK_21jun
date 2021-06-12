using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMovable
{
    void UpdateVelocity(float dt);
    Vector3 Velocity { get; set; }
    Vector3 Position { get; set; }
    Transform GfxTransform { get; }
}
