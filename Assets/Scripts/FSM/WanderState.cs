using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderState : FSMState<CompanionController>
{
    Wander wander;
    
    public override void EnterState(CompanionController companion)
    {
        Debug.Log("Entered Wander State");

        wander = companion.GetComponent<Wander>();
        wander.isActive = true;
    }

    public override void Execute(CompanionController companion)
    {
        Debug.Log("Wander State Running");

        if (Input.GetKeyDown(KeyCode.F))
        {
            companion.ChangeState(new IdleState());
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            companion.ChangeState(new FollowPlayerState());
        }
        else if (companion.hunger >= companion.maxHunger)
        {
            companion.ChangeState(new HungerState());
        }
        else if (companion.isHealthy == false /*&& NO ENEMIES AROUND*/)
        {
            companion.ChangeState(new FindHealthState());
        }
        else if (companion.fieldOfView.visibleTargets.Count > 0)
        {
            companion.ChangeState(new AttackState());
        }
    }

    public override void ExitState(CompanionController companion)
    {
        Debug.Log("Exited Wander State");
        wander.isActive = false;
    }
}
