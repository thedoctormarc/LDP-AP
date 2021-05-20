using NodeCanvas.Framework;
using UnityEngine;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;

public class T_WanderSocializer : ActionTask
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


        if (path.reachedDestination)
        {
            RandomRelocate();
        }

        aIPerception.VisualDetection(false, true);
        aIPerception.AuditiveDetection(true);

        GameObject closestAudio = agent.gameObject;
        GameObject closestVisual = agent.gameObject;

        if (aIPerception._audioDetected().Count > 0)
        {
            closestAudio = FindClosestTarget(aIPerception._audioDetected());
        }
  
        if (aIPerception._visuallyDetected().Count > 0)
        {
            closestVisual = FindClosestTarget(aIPerception._visuallyDetected());
        }

        GameObject closest = agent.gameObject;

        if (closestAudio != agent.gameObject || closestVisual != agent.gameObject)
        {
            if (closestAudio == agent.gameObject)
            {
                closest = closestVisual;
            }

            else if (closestVisual == agent.gameObject)
            {
                closest = closestAudio;
            }

            else
            {
                float distAudio = (closestAudio.transform.position - agent.transform.position).magnitude;
                float distVisual = (closestVisual.transform.position - agent.transform.position).magnitude;

                closest = (distAudio < distVisual) ? closestAudio : closestVisual;
            }

            // For the moment, go to the ally's position

            path.destination = closest.transform.position;
            bb.SetValue("lastTarget", path.destination);
        }

    }

    // find closest ally detected (visual or audio) that is aggroing an enemy
    GameObject FindClosestTarget(List<GameObject> detected)
    {
        GameObject ret = agent.gameObject;
        float closest = float.MaxValue;

        for (int i = 0; i < detected.Count; ++i)
        {
            GameObject go = detected[i];

            if (go.GetComponent<Blackboard>().GetValue<bool>("aggro") == false)
            {
                continue;
            }

            float dist = (go.transform.position - agent.transform.position).magnitude;

            if (dist < closest)
            {
                closest = dist;
                ret = go;
            }
        }

        return ret;
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
