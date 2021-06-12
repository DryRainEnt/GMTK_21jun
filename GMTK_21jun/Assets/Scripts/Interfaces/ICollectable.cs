using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICollectable
{
    int Index { get; set; }
    ICollector Collector { get; set; }
    void Fly(Vector3 targetPos);
    void Collected(ICollector target);
    void GetReady();
}