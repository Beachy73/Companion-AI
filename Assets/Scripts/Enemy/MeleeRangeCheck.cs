using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeRangeCheck : MonoBehaviour
{
    public bool inMeleeRange = false;
    public GameObject attackTarget;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Companion"))
        {
            inMeleeRange = true;
            attackTarget = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Companion"))
        {
            inMeleeRange = false;
            attackTarget = null;
        }
    }
}
