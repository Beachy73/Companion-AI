using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FleeState : FSMState<CompanionController>
{
    private Evade evade;
    private Perception perception;
    
    public override void EnterState(CompanionController companion)
    {
        evade = companion.GetComponent<Evade>();
        perception = companion.GetComponent<Perception>();

        evade.isActive = true;

        if (companion.enemyNear)
        {
            evade.pursuer = GetLastKnownEnemy().GetComponent<Vehicle>();
        }
        
    }

    public override void Execute(CompanionController companion)
    {
        Debug.Log("Evade from: " + evade.pursuer);
        
        if (evade.pursuer == null)
        {
            companion.ChangeState(new FindHealthState());
        }

        if (!companion.enemyNear)
        {
            companion.ChangeState(new FollowPlayerState());
        }
    }

    public override void ExitState(CompanionController companion)
    {
        evade.isActive = false;
    }

    private GameObject GetLastKnownEnemy()
    {
        GameObject[] sensedObjects = new GameObject[perception.memoryMap.Keys.Count];

        if (sensedObjects.Length > 0)
        {
            perception.memoryMap.Keys.CopyTo(sensedObjects, 0);
            GameObject entity = sensedObjects[0];
            return entity;
        }
        else
        {
            return null;
        }
        
        //MemoryRecord[] sensedRecord = new MemoryRecord[perception.memoryMap.Values.Count];

        //if (sensedRecord.Length > 0)
        //{
        //    perception.memoryMap.Values.CopyTo(sensedRecord, 0);
        //    GameObject[] sensedObjects = new GameObject[perception.memoryMap.Keys.Count];
        //    perception.memoryMap.Keys.CopyTo(sensedObjects, 0);

        //    double latestTime = Mathf.Infinity;
        //    GameObject lastKnownEnemy = null;

        //    for (int i = 0; i < sensedRecord.Length; i++)
        //    {
        //        TimeSpan interval = DateTime.Now - sensedRecord[i].timeLastSensed;
        //        //Debug.Log("Interval = " + interval);
        //        double curTime = interval.TotalSeconds;

        //        if (curTime < latestTime)
        //        {
        //            lastKnownEnemy = sensedObjects[i];
        //        }
        //    }

        //    return lastKnownEnemy;
        //}
        //else
        //{
        //    return null;
        //}
    }
}
