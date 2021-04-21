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
        Relocate();

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


        aIPerception.VisualDetection();
        aIPerception.AuditiveDetection();

        if (path.reachedDestination)
        {
            Relocate();
        }
    }

    // Right now, search a random active point pickup position (TODO: implement precision about the location depending on how many times the AI played the level)
    void Relocate() // TODO: search visible pickups first! If not, then random
    {
        GameObject points = GameObject.Find("Points");
        List<GameObject> activePoints = new List<GameObject>();

        for (int i = 0; i < points.transform.childCount; ++i)
        {
            GameObject child = points.transform.GetChild(i).gameObject;
            if (child.GetComponent<Pickup>().Active())
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