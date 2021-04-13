using NodeCanvas.Framework;
using UnityEngine;
using Pathfinding;

public class T_Die : ActionTask
{
    AIPath path;
    AILogic aILogic;

    protected override string OnInit()
    {
        path = agent.gameObject.GetComponent<AIPath>();
        aILogic = agent.gameObject.GetComponent<AILogic>();

        return null;
    }

    protected override void OnExecute()
    {
        agent.gameObject.GetComponent<AILogic>()._weaponSlot().transform.GetChild(0).gameObject.SetActive(false); // hide weapon for the moment
        path.canMove = false;
        path.canSearch = false;
        aILogic.currentState = AILogic.AI_State.die;
    }


    protected override void OnUpdate()
    {
        
    }

}