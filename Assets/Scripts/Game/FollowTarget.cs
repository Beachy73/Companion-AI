using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    private Transform target;

    public float smoothSpeed = 0.125f;
    public Vector3 offset;
    private Vector3 velocity = Vector3.zero;

    private void Awake()
    {
        target = FindObjectOfType<PlayerController>().transform;
    }

    private void LateUpdate()
    {
        Vector3 desiredPos = target.position + offset;
        Vector3 smootedPos = Vector3.SmoothDamp(transform.position, desiredPos, ref velocity, smoothSpeed);
        transform.position = smootedPos;
        transform.LookAt(target);
    }
}
