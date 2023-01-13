using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flee : SteeringBehaviourBase
{
    public Vector3 fleeTargetPoint;
    
    public override Vector3 Calculate()
    {
        Vehicle vehicle = GetComponent<Vehicle>();
        Vector3 desiredVelocity = (transform.position - fleeTargetPoint).normalized * vehicle.maxSpeed;

        // @TODO Stop the jittery movement when the capsule reaches the target location

        return (desiredVelocity - vehicle.velocity);
    }
}
