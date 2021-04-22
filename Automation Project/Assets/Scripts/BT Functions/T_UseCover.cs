using NodeCanvas.Framework;
using UnityEngine;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;

public class T_UseCover : ActionTask
{
    AIPath path;
    AILogic aILogic;
    Animator animator;
    AIPerception aIPerception;
    AIParameters aIParameters;
    Blackboard bb;
    GameObject enemy;

    // Start is called before the first frame update
    protected override string OnInit()
    {
        path = agent.gameObject.GetComponent<AIPath>();
        aILogic = agent.gameObject.GetComponent<AILogic>();
        aIPerception = agent.gameObject.GetComponent<AIPerception>();
        animator = agent.gameObject.GetComponent<Animator>();
        aIParameters = agent.gameObject.GetComponent<AIParameters>();
        bb = agent.gameObject.GetComponent<Blackboard>();

        return null;
    }

    protected override void OnExecute()
    {
        enemy = PlayerManager.instance.GetChildByIndex(aILogic._lastAggro());


        // search new position. If not found, just fight
        if (SearchSuitablePosition(true) == false)
        {
            return;
        };

        path.canMove = true;
        path.canSearch = true;
        aILogic.currentState = AILogic.AI_State.run;
        animator.SetInteger("Moving", 2);
        aILogic.RelocateWeapon();
        path.maxSpeed = aIParameters._runSpeed();

    }

    bool SearchSuitablePosition(bool doEndAction)
    {
        var grid = AstarPath.active.data.gridGraph;
     
        int i = 0;
        GraphNode rNode;
        do
        {
            rNode = grid.nodes[Random.Range(0, grid.nodes.Length)];
            ++i;
        }
        while (PositionSuitable(rNode, enemy) == false && i <= grid.nodes.Length);

        // no position found
        if (i >= grid.nodes.Length)
        {
            if (doEndAction)
            {
                EndAction(true);
            }

            return false;
        }

        path.destination = (Vector3)rNode.position;
        bb.SetValue("lastTarget", path.destination);

        return true;
    }

    bool PositionSuitable(GraphNode node, GameObject enemy)
    {
        // Not Walkable
        if (node.Walkable == false)
        {
            return false;
        }

        Vector3 enemyPos = enemy.transform.position;
        Vector3 diff = agent.transform.position - enemyPos;
        Vector3 dir =  diff.normalized;
        Vector3 nodePos = (Vector3)node.position;

        // I am too close to enemy
        float distToEnemy = (agent.transform.position - enemyPos).magnitude;
        
        if (distToEnemy < aIParameters._rifleCoverMinTriggerDist())
        {
            return false;
        }

        float nodeToPos = (nodePos - agent.transform.position).magnitude;
        float nodeToEnemy = (nodePos - enemyPos).magnitude;


        // Too far
        if (nodeToPos > aIParameters._rifleCoverMaxDist())
        {
            return false;
        }

        // Too close
        float marginalDistance = 3f;
        if (nodeToPos < marginalDistance)
        {
            return false;
        }

        // Too far from enemy
        if (nodeToEnemy > distToEnemy)
        {
            return false;
        }

        // Too far from player in comparison to enemy
        float percentage = 0.2f;

        if (nodeToPos / nodeToEnemy > percentage)
        {
            return false;
        }


        int nHits = aIPerception.LOF_FromNodePos((Vector3)node.position, enemy);
        int maxHits = aIPerception._raycastTargetOffsets().Length;

        // No cover
        if (nHits == 0)
        {
            return false;
        }

        // Total cover (no Line Of Fire)
        if (nHits == maxHits)
        {
            return false;
        }

        // Less cover than current cover!!
       /* int cHits = aIPerception.LOF_FromNodePos(AstarPath.active.GetNearest(agent.transform.position, NNConstraint.Default).position, enemy);

        if (cHits > nHits)
        {
            return false;
        }*/

        return true;
    }

    protected override void OnUpdate()
    {

        float distToEnemy = (agent.transform.position - enemy.transform.position).magnitude;
        float marginalDistance = 5f;

        bool c1 = bb.GetValue<bool>("dead");
        bool c2 = !bb.GetValue<bool>("aggro");
        bool c3 = path.reachedDestination;
  //      bool c4 = aIPerception.LostAggroLOF();
        bool c4 = distToEnemy <= marginalDistance;

        if (c1 || c2 || c3 || c4)
        {
            path.maxSpeed = aIParameters._walkSpeed();
            EndAction(true);
        }

    }
 
}
