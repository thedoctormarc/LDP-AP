using NodeCanvas.Framework;
using UnityEngine;
using Pathfinding;

public class T_Die : ActionTask
{
    AIPath path;
    AILogic aILogic;
    AIParameters aIParameters;
    AIPerception aIPerception;
    Blackboard bb;
    Animator animator;
    float currentTime;

    protected override string OnInit()
    {
        path = agent.gameObject.GetComponent<AIPath>();
        aILogic = agent.gameObject.GetComponent<AILogic>();
        aIPerception = agent.gameObject.GetComponent<AIPerception>();
        animator = agent.gameObject.GetComponent<Animator>();
        bb = agent.gameObject.GetComponent<Blackboard>();
        aIParameters = agent.gameObject.GetComponent<AIParameters>();

        return null;
    }

    protected override void OnExecute()
    {
        currentTime = 0f;
        path.canMove = false;
        path.canSearch = false;
        aILogic.currentState = AILogic.AI_State.die;

    }


    protected override void OnUpdate()
    {
        if((currentTime += Time.deltaTime) >= aIParameters._respawnTime())
        {
            Respawn();
        }
    }

    void Respawn()
    {
        // Reposition
        var grid = AstarPath.active.data.gridGraph;
        int threshold = 150;

        GraphNode rNode = grid.GetNearest(agent.transform.position).node;

        // Try to find a position without line of sight to enemy
        bool found = false;

        for (int i = 0; i < threshold; ++i)
        {
            rNode = grid.nodes[Random.Range(0, grid.nodes.Length)];

            if (rNode.Walkable == true && aIPerception.LOF_FromNodePos((Vector3)rNode.position) == false)
            {
                found = true;
                break;
            }
        }

        // If not found just try to find a walkable position
        if (found == false)
        {
            for (int i = 0; i < threshold; ++i)
            {
                rNode = grid.nodes[Random.Range(0, grid.nodes.Length)];

                if (rNode.Walkable == true)
                {
                    break;
                }
            }
        }

        agent.transform.position = (Vector3)rNode.position;

        // Reset
        animator.SetBool("Dead", false);
        bb.SetValue("dead", false);
        aILogic.currentState = AILogic.AI_State.idle;
        aIParameters.ResetHealth();

        // End action
        EndAction(true);
    }

}