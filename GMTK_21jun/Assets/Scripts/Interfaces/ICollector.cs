using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollector
{
    Transform Transform { get; }

    void Collect(ICollectable target);
    void Throw(ICollectable target, Vector3 pos);
}
