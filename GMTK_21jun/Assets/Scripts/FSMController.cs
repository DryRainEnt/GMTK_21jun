using System;
using System.Collections.Generic;
using UnityEngine;

public class FSMController<T> where T : Enum
{
    private Dictionary<string, FSMState<T>> _fsmStates = new Dictionary<string, FSMState<T>>();

    public FSMState<T> CurrentBehaviour { get; set; }

    public virtual void AddState(FSMState<T> state)
    {
        var stateId = state.GetStateId();

        if (!_fsmStates.ContainsKey(stateId))
        {
            _fsmStates[stateId] = state;
            CurrentBehaviour = state;
        }
        else
        {
            Debug.LogError($"Already added in {GetType().Name}: {state}");
        }
    }

    public virtual void RemoveState(FSMState<T> state)
    {
        var stateId = state.GetStateId();

        if (_fsmStates.ContainsKey(stateId))
        {
            _fsmStates.Remove(stateId);
        }
        else
        {
            Debug.LogError($"Not exists in {GetType().Name}: {state}");
        }
    }

    public virtual void DoTransition(T transition)
    {
        var stateId = CurrentBehaviour.GetOutputStateId(transition);
        var state = _fsmStates[stateId];

        // state.BeforeTransition();
        CurrentBehaviour = state;
        // state.AfterTransition();
    }
}
