using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class FSMState<T> where T : Enum
{
    protected Dictionary<T, string> _stateMap = new Dictionary<T, string>();

    public abstract void CheckCondition(GameObject actor, GameObject target);

    public abstract void Act(GameObject actor, GameObject target);

    public abstract string GetStateId();

    public virtual string GetOutputStateId(T transition)
    {
        if (_stateMap.TryGetValue(transition, out var stateId))
        {
            return stateId;
        }
        return null;
    }

    //public abstract void BeforeTransition();

    //public abstract void AfterTransition();
}
