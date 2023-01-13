using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public float projectileSpeed = 20f;

    private PlayerController player;
    private Vector3 playerPos;
    public float projectileDespawnDist = 20f;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        playerPos = player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * projectileSpeed * Time.deltaTime;

        if (Vector3.Distance(transform.position, playerPos) >= projectileDespawnDist)
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
