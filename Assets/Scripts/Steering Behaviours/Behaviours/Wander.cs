using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : SteeringBehaviourBase
{
    public float wanderRadius = 10f;
    public float wanderDistance = 10f;
    public float wanderJitter = 1f;

    private Vector3 wanderTarget = Vector3.zero;
    private float wanderAngle = 0.0f;

    public bool isActive = false;

    private void Start()
    {
        wanderAngle = Random.Range(0.0f, Mathf.PI * 2);
        wanderTarget = new Vector3(Mathf.Cos(wanderAngle), 0, Mathf.Sin(wanderAngle)) * wanderRadius;
    }

    public override Vector3 Calculate()
    {
        if (!isActive)
        {
            Vehicle vehicle = GetComponent<Vehicle>();
            //vehicle.velocity = new Vector3(0, 0, 0);

            return vehicle.velocity;
        }

        wanderAngle += Random.Range(-wanderJitter, wanderJitter);
        wanderTarget = new Vector3(Mathf.Cos(wanderAngle), 0, Mathf.Sin(wanderAngle)) * wanderRadius;

        Vector3 targetLocal = wanderTarget;

        Vector3 targetWorld = transform.position + wanderTarget;

        targetWorld += transform.forward * wanderDistance;

        return targetWorld - transform.position;
    }
}
