using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pursuit : SteeringBehaviourBase
{
    public Vehicle evader;

    // If evader is player, get Player Controller
    PlayerController evaderPc = null;

    public bool isActive = false;
    
    public override Vector3 Calculate()
    {
        Vehicle vehicle = GetComponent<Vehicle>();

        if (!isActive)
        {
            //vehicle.velocity = new Vector3(0, 0, 0);
            
            return vehicle.velocity;
        }

        if (evader.gameObject == GameObject.FindGameObjectWithTag("Player"))
        {
            evaderPc = evader.GetComponent<PlayerController>();
            //Debug.Log("Has EvaderPC");
        }
        
        Vector3 desiredVelocity = Vector3.zero;
        Vector3 toEvader = evader.transform.position - transform.position;
        float relativeHeading = Vector3.Dot(transform.forward.normalized, evader.transform.forward.normalized);

        if (relativeHeading == 1)
        {
            desiredVelocity = (evader.transform.position - transform.position).normalized * vehicle.maxSpeed;
            return (desiredVelocity - vehicle.velocity);
        }
        else
        {
            float lookAheadTime = toEvader.magnitude / (vehicle.maxSpeed + evader.maxSpeed);

            Vector3 evaderFuturePos = Vector3.zero;

            if (evaderPc != null)
            {
                evaderFuturePos = evader.transform.position + evaderPc.GetVelocity() * lookAheadTime;
                //Debug.Log("Evader velocity: " + evaderPc.GetVelocity());
            }
            else
            {
                evaderFuturePos = evader.transform.position + evader.velocity * lookAheadTime;
            }
            
            desiredVelocity = (evaderFuturePos - transform.position).normalized * vehicle.maxSpeed;

            return (desiredVelocity - vehicle.velocity);
        }
    }
}
