using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed;

    private Transform player;
    private Vector3 target;
    private Vector3 defaultLoc = new Vector3(0, 1, 0);

    private EnemyBB enemyBB;
    
    // Start is called before the first frame update
    void Start()
    {
        // @TODO CHANGE TO CLOSEST ENEMY POSITION FROM ENEMYBB
        player = FindObjectOfType<PlayerController>().transform;
        //enemyBB = FindObjectOfType<EnemyController>().GetComponent<EnemyBB>();
        enemyBB = transform.parent.gameObject.GetComponent<EnemyBB>();

        //target = enemyBB.closestEntity.transform.position;

        if (enemyBB.closestEntity != null)
        {
            //Debug.Log("Using closest entity");
            target = enemyBB.closestEntity.transform.position;
        }
        else
        {
            //target = defaultLoc;
            target = player.position;
        }
        target.y = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Target position = " + target.ToString());
        
        //Debug.Log("Projectile moving");
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (transform.position == target)
        {
            DestroyProjectile();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Companion"))
        {
            HealthManager hm = other.GetComponent<HealthManager>();
            hm.ChangeHealth(-20);
            DestroyProjectile();
        }
    }

    void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
