﻿using NodeCanvas.Framework;
using UnityEngine;
using Pathfinding;

public class T_Relocate : ActionTask
{
    AIPath path;
    AIPerception aIPerception;
    Animator animator;

    protected override string OnInit()
    {
        path = agent.gameObject.GetComponent<AIPath>();  
        aIPerception = agent.gameObject.GetComponent<AIPerception>();
        animator = agent.gameObject.GetComponent<Animator>();

        Relocate();

        return null;
    }

    protected override void OnUpdate()
    {
        aIPerception.VisualPerception();

        if(path.reachedDestination)
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
        animator.SetBool("Moving", true);
    }
 
}