using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrive : SteeringBehaviourBase
{
    public Vector3 arriveTargetPos;
    public float slowingThreshold = 10.0f;
    
    public override Vector3 Calculate()
    {
        Vehicle vehicle = GetComponent<Vehicle>();
        Vector3 toTarget = (arriveTargetPos - transform.position);
        Vector3 desiredVelocity = Vector3.zero;

        float distance = toTarget.magnitude;

        if (distance < slowingThreshold)
        {
            float speed = distance / slowingThreshold;
            speed = Mathf.Clamp(speed, 0.0f, vehicle.maxSpeed);
            desiredVelocity = toTarget.normalized * (speed / distance);

            return (desiredVelocity - vehicle.velocity);
        }
        else
        {
            desiredVelocity = (arriveTargetPos - transform.position).normalized * vehicle.maxSpeed;
            return (desiredVelocity - vehicle.velocity);
        }
    }
}
