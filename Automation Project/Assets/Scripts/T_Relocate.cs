using NodeCanvas.Framework;
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
        path.destination = new Vector3(14f, 0f, 99f);
        aIPerception = agent.gameObject.GetComponent<AIPerception>();
        animator = agent.gameObject.GetComponent<Animator>();

        animator.SetBool("Moving", true);

        return null;
    }

    protected override void OnUpdate()
    {
        aIPerception.VisualPerception();
    }
 
}