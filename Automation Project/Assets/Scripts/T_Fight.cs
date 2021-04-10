using NodeCanvas.Framework;
using UnityEngine;
using Pathfinding;

public class T_Fight : ActionTask
{
    AIPath path;

    protected override string OnInit()
    {
        path = agent.gameObject.GetComponent<AIPath>();
        path.canMove = false;
        path.canSearch = false;

        return null;
    }

    protected override void OnExecute()
    {
 
    }
}