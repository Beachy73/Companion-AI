using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitState : FSMState<CompanionController>
{
    private Idle idle;
    private GameObject player;
    public float damping = 2f;

    public override void EnterState(CompanionController companion)
    {
        Debug.Log("Entered wait state");

        idle = companion.GetComponent<Idle>();
        idle.isActive = true;

        player = companion.player;
    }

    public override void Execute(CompanionController companion)
    {
        Debug.Log("Waiting");

        Vector3 lookPos = player.transform.position - companion.transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        companion.transform.rotation = Quaternion.Slerp(companion.transform.rotation, rotation, Time.deltaTime * damping);

        // Companion doesn't get hungrier when idle
        companion.hunger -= 1 * Time.deltaTime;

        // State Changes
        if (companion.hitPlayerSphere == false)
        {
            companion.ChangeState(new FollowPlayerState());
        }
        else if (!companion.isHealthy && !companion.enemyNear /*&& NO ENEMIES AROUND*/)
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
    }

    public override void ExitState(CompanionController companion)
    {
        Debug.Log("Exited wait state");
        idle.isActive = false;
    }
}
