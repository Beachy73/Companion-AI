using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Evade : SteeringBehaviourBase
{
    public Vehicle pursuer;

    public bool isActive = false;
    
    public override Vector3 Calculate()
    {
        Vehicle vehicle = GetComponent<Vehicle>();

        if (!isActive)
        {
            return vehicle.velocity;
        }

        Vector3 desiredVelocity = Vector3.zero;
        Vector3 toPursuer = pursuer.transform.position - transform.position;
        //float relativeHeading = Vector3.Dot(transform.forward.normalized, pursuer.transform.forward.normalized);

        float lookAheadTime = toPursuer.magnitude / (vehicle.maxSpeed + pursuer.maxSpeed);
        Vector3 pursuerFuturePos = pursuer.transform.position + pursuer.velocity * lookAheadTime;

        desiredVelocity = (transform.position - pursuerFuturePos).normalized * vehicle.maxSpeed;

        return (desiredVelocity - vehicle.velocity);
    }
}
