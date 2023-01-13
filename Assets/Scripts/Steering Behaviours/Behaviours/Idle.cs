using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : SteeringBehaviourBase
{
    private Vehicle vehicle;
    public bool isActive;

    public override Vector3 Calculate()
    {
        vehicle = GetComponent<Vehicle>();

        if (!isActive)
        {
            return vehicle.velocity;
        }
        
        return vehicle.velocity = Vector3.zero;
    }
}
