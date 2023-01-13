using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An abstract class, steering behaviours will inherit from this
/// </summary>
[RequireComponent(typeof(Vehicle))]
public abstract class SteeringBehaviourBase : MonoBehaviour
{
    public abstract Vector3 Calculate();
}
