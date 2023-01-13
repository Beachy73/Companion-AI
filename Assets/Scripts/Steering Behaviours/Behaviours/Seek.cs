using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seek : SteeringBehaviourBase
{
    public Vector3 seekTargetPos;
    public bool isActive = false;
    
    public override Vector3 Calculate()
    {
        Vehicle vehicle = GetComponent<Vehicle>();

        if (!isActive)
        {
            return vehicle.velocity;
        }

        Vector3 desiredVelocity = (seekTargetPos - transform.position).normalized * vehicle.maxSpeed;

        // @TODO Stop the jittery movement when the capsule reaches the target location

        return (desiredVelocity - vehicle.velocity);
    }
}
