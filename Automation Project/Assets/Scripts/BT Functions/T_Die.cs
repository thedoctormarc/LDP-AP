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
    GraphUpdateScene gus;
    string gusTag;
    float currentTime;

    protected override string OnInit()
    {
        path = agent.gameObject.GetComponent<AIPath>();
        aILogic = agent.gameObject.GetComponent<AILogic>();
        aIPerception = agent.gameObject.GetComponent<AIPerception>();
        animator = agent.gameObject.GetComponent<Animator>();
        bb = agent.gameObject.GetComponent<Blackboard>();
        aIParameters = agent.gameObject.GetComponent<AIParameters>();
        gus = agent.transform.Find("GUS").GetComponent<GraphUpdateScene>();
     //   gusTag = gus.tag;

        return null;
    }

    protected override void OnExecute()
    {
        currentTime = 0f;
        path.canMove = false;
        path.canSearch = false;
        aILogic.currentState = AILogic.AI_State.die;
    //    gus.tag = "Basic Ground"; // TODO: on respawn, set to previous tag

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

        GraphNode rNode;
        do
        {
            rNode = grid.nodes[Random.Range(0, grid.nodes.Length)];
        }
        while (rNode.Walkable == false
        || aIPerception.LOF_FromNodePos((Vector3)rNode.position));

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