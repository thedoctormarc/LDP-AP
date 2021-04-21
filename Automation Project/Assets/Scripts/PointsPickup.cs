using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsPickup : Pickup
{
    [SerializeField]
    int points = 10;

    public override void Disappear(GameObject player)
    {
        player.GetComponent<AIParameters>().currentPoints += points;
        mRenderer.enabled = false;
    }
}
