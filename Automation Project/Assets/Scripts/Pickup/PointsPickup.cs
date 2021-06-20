using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Simulation.Games;

public class PointsPickup : Pickup
{
    [SerializeField]
    int points = 10;

    public override void Disappear(GameObject player)
    {
        Parameters parameters = player.GetComponent<Parameters>();
        parameters.currentPoints += points;
        string pointCounter = "T" + parameters._team().ToString() + " points";
        GameSimManager.Instance.IncrementCounter(pointCounter, (long)1);

        base.Disappear(player);
    }
}
