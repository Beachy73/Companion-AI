using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    #region Variables
    // Updated Values

    /// <summary>
    /// Applied to the current position every frame
    /// </summary>
    public Vector3 velocity;

    // Position, Heading and Side can be accessed through transform component
    // transform.position, transform.forward and transform.right respectively


    // Constant Values

    // Represents weight of object, affects acceleration
    public float mass = 1;

    // Maximum speed this agent can move per second
    public float maxSpeed = 1;

    // The thrust this agent can produce
    public float maxForce = 1;

    // How fast the agent can turn
    public float maxTurnRate = 1;


    private GameObject player = null;
    private Vehicle playerVehicle = null;
    #endregion

    private void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            playerVehicle = player.GetComponent<Vehicle>();
        }

        if (this == playerVehicle)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            maxSpeed = pc.moveSpeed;
        }
        
    }


    private void Update()
    {
        Vector3 steeringForce = Vector3.zero;

        //Get all steering behaviours attached to this object and add their calculated steering force onto this SteeringForce

        SteeringBehaviourBase[] behaviourBases = GetComponents<SteeringBehaviourBase>();
        Vector3 calculationAmount = Vector3.zero;

        for (int i = 0; i < behaviourBases.Length; i++)
        {
            calculationAmount = behaviourBases[i].Calculate();
            steeringForce += calculationAmount;
            calculationAmount = Vector3.zero;
        }


        Vector3 acceleration = steeringForce / mass;

        velocity += acceleration * Time.deltaTime;

        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        if (velocity != Vector3.zero)
        {
            transform.position += velocity * Time.deltaTime;

            transform.forward = velocity.normalized;
        }
    }
}
