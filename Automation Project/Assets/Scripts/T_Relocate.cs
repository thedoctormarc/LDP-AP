using NodeCanvas.Framework;
using UnityEngine;
using Pathfinding;

public class T_Relocate : ActionTask
{
    AIPath path;

    protected override string OnInit()
    {
        path = agent.gameObject.GetComponent<AIPath>();
        path.destination = new Vector3(14f, 0f, 99f);

        return null;
    }

    protected override void OnExecute()
    {

    }
}