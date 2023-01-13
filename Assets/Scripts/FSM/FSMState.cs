using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FSMState <T>
{
    public abstract void EnterState(T entity);

    public abstract void Execute(T entity);

    public abstract void ExitState(T entity);
}
