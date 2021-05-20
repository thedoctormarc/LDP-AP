using NodeCanvas.Framework;
using UnityEngine;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;

public class T_WanderKiller : ActionTask
{
    AIPath path;
    AIPerception aIPerception;
    AIParameters aIParameters;
    Animator animator;
    AILogic aILogic;
    Blackboard bb;

    protected override string OnInit()
    {
        path = agent.gameObject.GetComponent<AIPath>();
        path.canMove = true;
        path.canSearch = true;
        aIPerception = agent.gameObject.GetComponent<AIPerception>();
        aIParameters = agent.gameObject.GetComponent<AIParameters>();
        animator = agent.gameObject.GetComponent<Animator>();
        aILogic = agent.gameObject.GetComponent<AILogic>();
        bb = agent.gameObject.GetComponent<Blackboard>();
        Vector3 max = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        bb.SetValue("lastTarget", max);

        return null;
    }

    protected override void OnExecute()
    {
        animator.SetInteger("Moving", 1);
        path.canMove = true;
        path.canSearch = true;
        aILogic.currentState = AILogic.AI_State.walk;
        aILogic.RelocateWeapon();

        Vector3 lastTarget = bb.GetValue<Vector3>("lastTarget");
        if (lastTarget.x == float.MaxValue) // search new target if I didnt have one previous to a fight
        {
            RandomRelocate();
        }

    }


    // Update is called once per frame
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
            RandomRelocate();
        }

        if (aIPerception._audioDetected().Count > 0)
        {
            float closest = float.MaxValue;
            int index = 0;
            for (int i = 0; i < aIPerception._audioDetected().Count; ++i)
            {
                GameObject go = aIPerception._audioDetected()[i];
                float dist = (go.transform.position - agent.transform.position).magnitude;
                if (dist < closest)
                {
                    closest = dist;
                    index = i;
                }
            }

          
            GameObject detected = aIPerception._audioDetected()[index];

            path.destination = detected.transform.position;
            bb.SetValue("lastTarget", path.destination);
        }

    }


    void RandomRelocate()
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
