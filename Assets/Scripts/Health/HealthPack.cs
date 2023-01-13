using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPack : MonoBehaviour
{
    #region Variables

    private GameObject player;
    private SphereCollider playerSphere;

    [Range(0, 100)]
    public int healAmount = 20;

    public bool beenCollected = false;

    #endregion

    private void Awake()
    {
        player = FindObjectOfType<PlayerController>().gameObject;
        playerSphere = player.GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        HealthManager healthManager = other.GetComponent<HealthManager>();

        //Ignore Player Sphere used for telling companion AI to stop following
        if (other == playerSphere)
        {
            return;
        }
        else if (healthManager != null && healthManager.GetCurrentHealth() != healthManager.maxHealth)
        {
            healthManager.ChangeHealth(healAmount);
            beenCollected = true;
            //gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
