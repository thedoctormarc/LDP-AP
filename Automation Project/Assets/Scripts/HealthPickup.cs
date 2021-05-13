using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : Pickup
{
    [SerializeField]
    int health = 50;

    public override void Disappear(GameObject player)
    {
        Parameters aIParameters = player.GetComponent<Parameters>();
        float maxHealth = aIParameters._maxHealth();
        float currentHealth = aIParameters._currentHealth();
        currentHealth += health;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        aIParameters.UpdateHealth(currentHealth);
        mRenderer.enabled = false;
    }
}
