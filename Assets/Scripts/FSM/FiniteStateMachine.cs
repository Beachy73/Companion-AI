using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FiniteStateMachine <T>
{
    private T Owner;
    private FSMState<T> CurrentState;
    private FSMState<T> PreviousState;
    //private FSMState<T> GlobalState;
    
    public void Configure(T owner, FSMState<T> initialState)
    {
        Owner = owner;
        ChangeState(initialState);
        CurrentState = initialState;
    }
    
    public void Update()
    {
        //if (GlobalState != null) GlobalState.Execute(Owner);
        if (CurrentState != null)
        {
            CurrentState.Execute(Owner);
        }
    }

    public void ChangeState(FSMState<T> newState)
    {
        PreviousState = CurrentState;
        
        if (CurrentState != null)
        {
            CurrentState.ExitState(Owner);
            CurrentState = newState;

            if (CurrentState != null)
            {
                CurrentState.EnterState(Owner);
            }
        }
    }

    public void RevertToPreviousState()
    {
        if (PreviousState != null)
        {
            ChangeState(PreviousState);
        }
    }

    public FSMState<T> GetCurrentState()
    {
        return CurrentState;
    }

    public FSMState<T> GetPreviousState()
    {
        return PreviousState;
    }
}
