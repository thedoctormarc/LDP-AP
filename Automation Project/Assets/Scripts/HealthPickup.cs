using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : Pickup
{
    [SerializeField]
    int health = 50;

    public override void Disappear(GameObject player)
    {
        AIParameters aIParameters = player.GetComponent<AIParameters>();
        float maxHealth = aIParameters._maxHealth();
        float currentHealth = aIParameters.currentHealth;
        currentHealth += health;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        player.GetComponent<AIParameters>().currentHealth = currentHealth;
        mRenderer.enabled = false;
    }
}
