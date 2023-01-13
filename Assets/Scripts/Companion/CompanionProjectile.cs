using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionProjectile : MonoBehaviour
{
    public float projectileSpeed = 20f;

    private CompanionController companion;
    private Vector3 companionPos;

    private GameObject target;
    private Vector3 targetPos;

    // Start is called before the first frame update
    void Start()
    {
        companion = FindObjectOfType<CompanionController>();
        companionPos = companion.transform.position;

        target = companion.closestEnemy;
        if (target != null)
        {
            targetPos = target.transform.position;
        }
        targetPos.y = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position += transform.forward * projectileSpeed * Time.deltaTime;

        //if (Vector3.Distance(transform.position, companionPos) >= projectileDespawnDist)
        //{
        //    DestroyProjectile();
        //}

        transform.position = Vector3.MoveTowards(transform.position, targetPos, projectileSpeed * Time.deltaTime);

        if (transform.position == targetPos)
        {
            DestroyProjectile();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            HealthManager hm = other.GetComponent<HealthManager>();
            hm.ChangeHealth(-20);
            DestroyProjectile();
        }
    }

    private void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
