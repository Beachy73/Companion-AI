using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindHealthState : FSMState<CompanionController>
{
    private Seek seek;

    private List<HealthPack> healthPacks;
    private HealthPack closestPack;

    // Keep count of collected packs, if collected one revert to previous state?
    //private int noOfCollectedPacks = 0;

    public override void EnterState(CompanionController companion)
    {
        Debug.Log("Finding health pack");

        seek = companion.GetComponent<Seek>();
        seek.isActive = true;
    }

    public override void Execute(CompanionController companion)
    {
        // Revert back to previous state if companion is classed as healthy
        if (companion.isHealthy == true)
        {
            FiniteStateMachine<CompanionController> FSM = companion.GetFiniteStateMachine();
            Debug.Log("Current State = " + FSM.GetCurrentState());
            Debug.Log("Previous State = " + FSM.GetPreviousState());
            companion.RevertToPreviousState();

            //companion.ChangeState(new FollowPlayerState());
        }
        // Change to flee is enemy is close
        if (!companion.isHealthy && companion.enemyNear)
        {
            companion.ChangeState(new FleeState());
        }
        
        healthPacks = new List<HealthPack>();

        foreach (HealthPack hp in GameObject.FindObjectsOfType<HealthPack>())
        {
            healthPacks.Add(hp);
        }

        if (healthPacks.Count == 0)
        {
            //companion.ChangeState(new FollowPlayerState());
            companion.RevertToPreviousState();
        }
        else
        {
            Debug.Log("Health Pack List length: " + healthPacks.Count);

            closestPack = GetClosestHealthPack(healthPacks, companion);
            Debug.Log("Closest Pack = " + closestPack);

            seek.seekTargetPos = closestPack.transform.position;

            // Doesn't work as intended, the closest pack gets destroyed so AI seeks next closest pack
            //if (closestPack.beenCollected)
            //{
            //    Debug.Log("Been collected");
            //    companion.RevertToPreviousState();
            //}

            
        }
    }

    public override void ExitState(CompanionController companion)
    {
        Debug.Log("Exiting find health state");

        seek.isActive = false;
    }

    private HealthPack GetClosestHealthPack(List<HealthPack> healthPacks, CompanionController companion)
    {
        HealthPack closestPack = null;
        float distance = Mathf.Infinity;
        Vector3 pos = companion.transform.position;
        foreach (HealthPack hp in healthPacks)
        {
            Vector3 diff = hp.transform.position - pos;
            float curDistance = diff.sqrMagnitude;

            if (curDistance < distance)
            {
                closestPack = hp;
                distance = curDistance;
            }
        }

        return closestPack;
    }
}
