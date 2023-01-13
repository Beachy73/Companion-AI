using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : FSMState<CompanionController>
{
    private Idle idle;
    private FieldOfView fieldOfView;
    private GameObject projectilePrefab;

    private float timeBtwShots;

    public override void EnterState(CompanionController companion)
    {
        Debug.Log("Entered attack state");
        
        idle = companion.GetComponent<Idle>();
        fieldOfView = companion.GetComponent<FieldOfView>();
        projectilePrefab = companion.projectilePrefab;

        idle.isActive = true;

        timeBtwShots = 0f;
    }

    public override void Execute(CompanionController companion)
    {
        if (timeBtwShots <= 0)
        {
            Debug.Log("Companion Attacking");
            Fire(companion);
            timeBtwShots = companion.startTimeBtwShots;
        }
        else
        {
            timeBtwShots -= Time.deltaTime;
        }


        if (fieldOfView.visibleTargets.Count <= 0 && companion.isHealthy)
        {
            companion.RevertToPreviousState();
        }
        else if (fieldOfView.visibleTargets.Count > 0 && !companion.isHealthy)
        {
            companion.ChangeState(new FleeState());
        }
        else if (fieldOfView.visibleTargets.Count <= 0 && !companion.isHealthy)
        {
            companion.ChangeState(new FindHealthState());
        }
    }

    public override void ExitState(CompanionController companion)
    {
        Debug.Log("Exited attack state");
        idle.isActive = false;
    }

    private void Fire(CompanionController companion)
    {
        MonoBehaviour.Instantiate(projectilePrefab, companion.transform.position, Quaternion.identity);
    }
}
