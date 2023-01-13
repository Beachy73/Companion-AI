using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayerState : FSMState<CompanionController>
{
    private Pursuit pursuit;

    private GameObject player;
    private float followDelay = 0f;
    public float maxFollowDelay = 1f;
    public float speed = 10f;

    public override void EnterState(CompanionController companion)
    {
        Debug.Log("Entered Following Player State");

        followDelay = 0f;

        pursuit = companion.GetComponent<Pursuit>();
        player = companion.player;

        //pursuit.isActive = true;
    }

    public override void Execute(CompanionController companion)
    {
        Debug.Log("Following Player!");

        followDelay += 1 * Time.deltaTime;

        // Keeping companion rotated towards player before following again
        if (pursuit.isActive == false && followDelay >= maxFollowDelay)
        {
            pursuit.isActive = true;
        }
        else if (pursuit.isActive == false && followDelay < maxFollowDelay)
        {
            Vector3 lookPos = player.transform.position - companion.transform.position;
            lookPos.y = 0;
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            companion.transform.rotation = Quaternion.Slerp(companion.transform.rotation, rotation, Time.deltaTime * speed);
        }

        
        

        // State Changes
        if (Input.GetKeyDown(KeyCode.F))
        {
            companion.ChangeState(new IdleState());
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            companion.ChangeState(new WanderState());
        }
        else if (companion.hunger >= companion.maxHunger)
        {
            companion.ChangeState(new HungerState());
        }
        else if (companion.hitPlayerSphere)
        {
            companion.ChangeState(new WaitState());
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


        // Debugging
        if (Input.GetKeyDown(KeyCode.P))
        {
            companion.ChangeState(new IdleState());
        }
    }

    public override void ExitState(CompanionController companion)
    {
        Debug.Log("Exited Following Player State");
        pursuit.isActive = false;
    }

}







    //////////////////////////////////////////////////////////////////////////
    //////////////////////////First Implementation////////////////////////////
    //////////////////////////////////////////////////////////////////////////

//{
    ////private float posX;
    ////private float posY;
    ////private float posZ;

    ////private float playerPosX;
    ////private float playerPosY;
    ////private float playerPosZ;

    //private Vector3 pos;
    //private Vector3 playerPos;

    //public override void EnterState(CompanionController companion)
    //{
    //    Debug.Log("Entered following player state");
    //}

    //public override void Execute(CompanionController companion)
    //{
    //    //posX = companion.transform.position.x;
    //    //posY = companion.transform.position.y;
    //    //posZ = companion.transform.position.z;

    //    //playerPosX = companion.player.transform.position.x;
    //    //playerPosY = companion.player.transform.position.y;
    //    //playerPosZ = companion.player.transform.position.z;

    //    //posX = playerPosX;
    //    //posY = playerPosY;
    //    //posZ = playerPosZ;

    //    //companion.transform.position = new Vector3(posX, posY, posZ);

    //    pos.x = companion.transform.position.x;
    //    pos.y = companion.transform.position.y;
    //    pos.z = companion.transform.position.z;

    //    playerPos.x = companion.player.transform.position.x;
    //    playerPos.y = companion.player.transform.position.y;
    //    playerPos.z = companion.player.transform.position.z;

    //    companion.transform.position = Vector3.MoveTowards(pos, playerPos, 1f);

    //    //Debug.Log("Following Player!");

    //    //Debug.Log("PosX = " + posX);
    //    //Debug.Log("PosY = " + posY);
    //    //Debug.Log("PosZ = " + posZ);

    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        companion.ChangeState(new IdleState());
    //    }
    //}

    //public override void ExitState(CompanionController companion)
    //{
    //    Debug.Log("Exited following player state");
    //}
//}