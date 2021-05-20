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
        float currentHealth = aIParameters._currentHealth();
        float maxHealth = aIParameters._maxHealth();

        if (currentHealth == maxHealth)
            return;

        currentHealth += health;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        aIParameters.UpdateHealth(currentHealth);
        HumanController h = player.GetComponent<HumanController>();
        if (h)
        {
            h.UpdateHealthBar();
        }

        base.Disappear(player);
    }
}
