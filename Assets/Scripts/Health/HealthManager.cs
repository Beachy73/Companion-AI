using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    #region Variables

    [Range(0, 200)]
    public int maxHealth;
    private int currentHealth;

    public bool isHealthy;
    public int isHealthyLevel = 50;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        // @TODO Add in death code
        if (currentHealth <= 0)
        {
            gameObject.SetActive(false);
        }

        if (currentHealth <= isHealthyLevel)
        {
            isHealthy = false;
        }
        else
        {
            isHealthy = true;
        }
    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void SetMaxHealth()
    {
        currentHealth = maxHealth;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
