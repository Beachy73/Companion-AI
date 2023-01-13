using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HungerState : FSMState<CompanionController>
{
    private Idle idle;

    public override void EnterState(CompanionController companion)
    {
        Debug.Log("I'm hungry!");

        idle = companion.GetComponent<Idle>();
        idle.isActive = true;

        companion.hasReachedMaxHunger = true;
    }

    public override void Execute(CompanionController companion)
    {
        //Debug.Log("Companion Hunger = " + companion.hunger);
        
        if (companion.hunger >= 0)
        {
            companion.hunger -= 3 * Time.deltaTime;
        }
        else if (companion.hunger <= 0)
        {
            //companion.ChangeState(new FollowPlayerState());
            companion.RevertToPreviousState();
        }
    }

    public override void ExitState(CompanionController companion)
    {
        Debug.Log("Not hungry anymore");
        idle.isActive = false;

        companion.hunger = 0;
        companion.hasReachedMaxHunger = false;
    }
}
