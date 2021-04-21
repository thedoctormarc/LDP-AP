using NodeCanvas.Framework;
using UnityEngine;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;

public class T_WanderCollector : ActionTask
{
    AIPath path;
    AIPerception aIPerception;
    Animator animator;
    AILogic aILogic;
    Blackboard bb;

    protected override string OnInit()
    {
        path = agent.gameObject.GetComponent<AIPath>();
        path.canMove = true;
        path.canSearch = true;
        aIPerception = agent.gameObject.GetComponent<AIPerception>();
        animator = agent.gameObject.GetComponent<Animator>();
        aILogic = agent.gameObject.GetComponent<AILogic>();
        bb = agent.gameObject.GetComponent<Blackboard>();
        SearchRandomPickup();

        return null;
    }

    protected override void OnExecute()
    {
        animator.SetInteger("Moving", 1);
        path.canMove = true;
        path.canSearch = true;
        aILogic.currentState = AILogic.AI_State.walk;
        aILogic.RelocateWeapon();
    }


    protected override void OnUpdate()
    {

        if (bb.GetValue<bool>("dead"))
        {
            EndAction(true);
        }


        List<GameObject> detected = aIPerception.VisualDetection(true);
        int closestIndex = 0;
        float nearDist = float.MaxValue;
        bool targetFound = false;

        if (detected.Count > 0)
        {
            for (int i = 0; i < detected.Count; ++i)
            {
                GameObject go = detected[i];

                if (go.CompareTag("pickup"))
                {
                    if (go.GetComponent<Pickup>().pickupType == Pickup.Type.POINTS)
                    {
                        targetFound = true;
                        float dist = (go.transform.position - agent.transform.position).magnitude;

                        if (dist < nearDist)
                        {
                            closestIndex = i;
                        }
                    }
                }
            }

            if (targetFound)
            {
                path.destination = (Vector3)AstarPath.active.data.gridGraph.GetNearest(detected[closestIndex].transform.position).node.position;
            }
        }

        aIPerception.AuditiveDetection();

        if (path.reachedDestination)
        {
            SearchRandomPickup();
        }
    }

    // Right now, search a random active point pickup position (TODO: implement precision about the location depending on how many times the AI played the level)
    void SearchRandomPickup() // TODO: search visible pickups first! If not, then random
    {
        GameObject pickups = AppManager.instance._pickups();
        List<GameObject> activePoints = new List<GameObject>();

        for (int i = 0; i < pickups.transform.childCount; ++i)
        {
            GameObject child = pickups.transform.GetChild(i).gameObject;

            Pickup p = child.GetComponent<Pickup>();
            if (p.pickupType == Pickup.Type.POINTS && p.Active())
            {
                activePoints.Add(child);
            }
        }

        if (activePoints.Count == 0)
        {
            // TODO: do something about it??
        }

        int j = Random.Range(0, activePoints.Count - 1);
        var grid = AstarPath.active.data.gridGraph;
        GraphNode targetNode = grid.GetNearest(activePoints[j].transform.position).node;
        path.destination = (Vector3)targetNode.position;
        animator.SetInteger("Moving", 1);
    }


}