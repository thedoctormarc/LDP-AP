using NodeCanvas.Framework;
using UnityEngine;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;

public class T_ShotgunCloseGap : ActionTask
{
    AIPath path;
    AILogic aILogic;
    Animator animator;
    AIPerception aIPerception;
    AIParameters aIParameters;
    Blackboard bb;

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
    // Update is called once per frame
    protected override void OnExecute()
    {  
        // search for close position near enemy, run towards it
        Vector3 enemyPos = PlayerManager.instance.GetChildByIndex(aILogic._lastAggro()).transform.position;
        Vector3 dir = (agent.transform.position - enemyPos).normalized;
        Vector3 closePos = enemyPos + dir * aIParameters._shotgunTargetFightDist();

        // if position further than current position, end action and fight already
        if ((closePos - enemyPos).magnitude > (agent.transform.position - enemyPos).magnitude)
        {
            EndAction(true);
            return;
        }

        path.canMove = true;
        path.canSearch = true;
        aILogic.currentState = AILogic.AI_State.run;
        animator.SetInteger("Moving", 2);
        aILogic.RelocateWeapon();
        Vector3 closePosWalkable = AstarPath.active.GetNearest(closePos, NNConstraint.Default).position;
        path.destination = closePosWalkable;
        path.maxSpeed = aIParameters._runSpeed();
    }

    protected override void OnUpdate()
    {
        if (bb.GetValue<bool>("dead") || !bb.GetValue<bool>("aggro") || path.reachedDestination || aIPerception.LostAggroLOF())
        {
            path.maxSpeed = aIParameters._walkSpeed();
            EndAction(true);
        }

    }

}
