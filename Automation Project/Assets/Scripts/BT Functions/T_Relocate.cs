﻿using NodeCanvas.Framework;
using UnityEngine;
using Pathfinding;

public class T_Relocate : ActionTask
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


        aIPerception.VisualDetection(false);
        aIPerception.AuditiveDetection();

        if (path.reachedDestination)
        {
            Relocate();
        }
    }

    void Relocate()
    {
        var grid = AstarPath.active.data.gridGraph;
        float minDistance = 10f;

        GraphNode cNode = grid.GetNearest(agent.transform.position).node;
        GraphNode rNode;
        do
        {
            rNode = grid.nodes[Random.Range(0, grid.nodes.Length)];
        }
        while (rNode.Walkable == false
        || PathUtilities.IsPathPossible(cNode, rNode) == false
        || (rNode.position - cNode.position).magnitude <= minDistance);


        path.destination = (Vector3)rNode.position;
        bb.SetValue("lastTarget", path.destination);
        animator.SetInteger("Moving", 1);
    }


}