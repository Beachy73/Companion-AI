using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : FSMState<CompanionController>
{
    private Idle idle;
    //private FieldOfView fieldOfView;
    
    public override void EnterState(CompanionController companion)
    {
        Debug.Log("Entered Idle State");

        idle = companion.GetComponent<Idle>();
        idle.isActive = true;

    }

    public override void Execute(CompanionController companion)
    {
        Debug.Log("Idle State Running");

        // Need these when initialised for the first time?
        if (idle == null)
        {
            idle = companion.GetComponent<Idle>();
        }
        if (idle.isActive == false)
        {
            idle.isActive = true;
        }


        // Companion doesn't get hungrier when idle
        companion.hunger -= 1 * Time.deltaTime;

        //Debug.Log("Companion Field of View = " + fieldOfView);

        // State Changes
        if (!companion.isHealthy && !companion.enemyNear /*&& NO ENEMIES AROUND*/)
        {
            companion.ChangeState(new FindHealthState());
        }
        else if (!companion.isHealthy && companion.enemyNear)
        {
            companion.ChangeState(new FleeState());
        }
        else if (companion.fieldOfView.visibleTargets.Count > 0)
        {
            companion.ChangeState(new AttackState());
        }


        if (Input.GetKeyDown(KeyCode.F))
        {
            companion.ChangeState(new FollowPlayerState());
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            companion.ChangeState(new WanderState());
        }
        
        
        
        // Debug for new states:
        //if (Input.GetKeyDown(KeyCode.H))
        //{
        //    companion.ChangeState(new FindHealthState());
        //}
        //else if (Input.GetKeyDown(KeyCode.L))
        //{
        //    companion.ChangeState(new AttackState());
        //}
    }

    public override void ExitState(CompanionController companion)
    {
        Debug.Log("Exiting Idle State");
        idle.isActive = false;
    }
}
