using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using NodeCanvas.Framework;

public class T_WanderPriorities : ActionTask
{
    public enum taskType { NO_TYPE, HEALTH, POINTS, TEAMMATE, FIGHT }
    Dictionary<taskType, int> priorities;
    AIParameters.Player_Type pType;
    AIPerception aIPerception;
    Vector3 currentTarget;
    int currentPriority;
    List<System.Tuple<int, Vector3>> targetStack;
    AIPath path;
    AILogic aILogic;
    Animator animator;
    Blackboard bb;
    AIParameters aIParameters;

    // Start is called before the first frame update
    protected override string OnInit()
    {
        priorities = new Dictionary<taskType, int>();
        targetStack = new List<System.Tuple<int, Vector3>>();
        currentTarget = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        currentPriority = int.MaxValue;
        pType = agent.gameObject.GetComponent<AIParameters>()._pType();
        aIPerception = agent.gameObject.GetComponent<AIPerception>();
        aIParameters = agent.gameObject.GetComponent<AIParameters>();
        aILogic = agent.gameObject.GetComponent<AILogic>();
        path = agent.gameObject.GetComponent<AIPath>();
        animator = agent.gameObject.GetComponent<Animator>();
        bb = agent.gameObject.GetComponent<Blackboard>();

        switch (pType)
        {
            case AIParameters.Player_Type.collector:
                {
                    priorities.Add(taskType.POINTS, 0);
                    priorities.Add(taskType.HEALTH, 1);
                    priorities.Add(taskType.FIGHT, 2);
                    priorities.Add(taskType.TEAMMATE, 3);
                    break;
                }

            case AIParameters.Player_Type.killer:
                {
                    priorities.Add(taskType.POINTS, 4);
                    priorities.Add(taskType.HEALTH, 2);
                    priorities.Add(taskType.FIGHT, 1);
                    priorities.Add(taskType.TEAMMATE, 3);
                    break;
                }


            case AIParameters.Player_Type.socializer:
                {
                    priorities.Add(taskType.POINTS, 4);
                    priorities.Add(taskType.HEALTH, 2);
                    priorities.Add(taskType.FIGHT, 3);
                    priorities.Add(taskType.TEAMMATE, 1);
                    break;
                }
        }



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
        
        if (lastTarget.x == float.MaxValue) // If i died, reset all targets
        {
            currentTarget = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            currentPriority = int.MaxValue;
            targetStack.Clear(); 
        }

        SearchNewTarget();

    }

    protected override void OnUpdate()
    {
        if (targetStack.Count > 4)
        {
            Debug.LogError("AI target stack surpassed the maximum");
        }

        if (bb.GetValue<bool>("dead"))
        {
            EndAction(true);
        }


        if (path.reachedDestination)
        {
            RemoveLastTarget();
            SearchNewTarget();
        }

        aIPerception.VisualDetection(true, true);
        aIPerception.AuditiveDetection();

        List<GameObject> visuallyDetected = aIPerception._visuallyDetected();
        List<GameObject> audioDetected = aIPerception._audioDetected();

        VisualScan(visuallyDetected);
        AuditiveScan(audioDetected);
    }

    void RemoveLastTarget()
    {
        for (int i = 0; i < targetStack.Count; ++i)
        {
            if (targetStack[i].Item1 == currentPriority)
            {
                targetStack.RemoveAt(i);
            }
        }

        currentTarget = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        currentPriority = int.MaxValue;
    }

    // Adds either a pickup task or a teammate task, the latter only if a teammate is fighting
    void VisualScan(List<GameObject> detected)
    {
        if (detected.Count == 0)
        {
            return;
        }


        // Search target
        for (int i = 0; i < detected.Count; ++i)
        {
            GameObject go = detected[i];
            taskType go_TaskType = taskType.NO_TYPE;

            if (go.CompareTag("pickup"))
            {
                go_TaskType = (taskType)System.Enum.Parse(typeof(taskType), go.GetComponent<Pickup>().pickupType.ToString());
            }
            else if (go.transform.parent.gameObject.CompareTag("Player"))
            {
                if (go.GetComponent<Blackboard>().GetValue<bool>("aggro") == false)
                {
                    continue;
                }

                if (go.GetComponent<AIParameters>()._team() != aIParameters._team())
                {
                    continue;
                }

                go_TaskType = taskType.TEAMMATE;
            }

            // Add the new target with its priority
            if (go_TaskType != taskType.NO_TYPE)
            {
                System.Tuple<int, Vector3> target = new System.Tuple<int, Vector3>(priorities[go_TaskType], go.transform.position);
                InsertTarget(target);
            }
        
        }

        // Refresh new target
        SearchNewTarget();

    }

    // Adds a fight task if an enemy is auditively detected
    void AuditiveScan(List<GameObject> detected)
    {
        if (detected.Count == 0)
        {
            return;
        }

        float closest = float.MaxValue;
        int index = 0;
        for (int i = 0; i < detected.Count; ++i)
        {
            GameObject go = detected[i];
            float dist = (go.transform.position - agent.transform.position).magnitude;
            if (dist < closest)
            {
                closest = dist;
                index = i;
            }
        }

        GameObject closestGo = aIPerception._audioDetected()[index];
        System.Tuple<int, Vector3> target = new System.Tuple<int, Vector3>(priorities[taskType.FIGHT], closestGo.transform.position);
        InsertTarget(target);

        // Refresh new target
        SearchNewTarget();

    }

    void InsertTarget(System.Tuple<int, Vector3> target)
    {

        for (int i = 0; i < targetStack.Count; ++i)
        {
            if (targetStack[i].Item1 == target.Item1) // same priority target, keep the one closest
            {

                if (targetStack[i].Item2 == target.Item2) // same target. do nothing
                {
                    return;
                }

                float dist = (targetStack[i].Item2 - agent.transform.position).magnitude;
                float newDist = (target.Item2 - agent.transform.position).magnitude;

                if (newDist < dist)
                {
                    targetStack[i] = target;
                }

                return;
            }
        }

        targetStack.Add(target);

        if (targetStack.Count == 1)
        {
            currentPriority = target.Item1;
        }
    }

    void SearchNewTarget() // Called at the beginning and when reached destination
    {
        // No targets active, search random destination
        if (targetStack.Count == 0)
        {
            RandomRelocate();
            return;
        }

        int priority = int.MaxValue;
        int index = int.MaxValue;

        // Search highest priority target
        for (int i = 0; i < targetStack.Count; ++i)
        {
            System.Tuple<int, Vector3> t = targetStack[i];

            if (t.Item1 < priority)
            {
                priority = t.Item1;
                index = i;
            }
        }

        // set destination to the target
        var grid = AstarPath.active.data.gridGraph;
        GraphNode node = grid.GetNearest(targetStack[index].Item2, NNConstraint.Default).node;
        path.destination = (Vector3)node.position;
        bb.SetValue("lastTarget", path.destination);

        // Set new current target and priority 
        currentPriority = priority;
        currentTarget = path.destination;
    }


    void RandomRelocate()
    {
        // Reposition
        var grid = AstarPath.active.data.gridGraph;
        int threshold = 150;

        GraphNode rNode = grid.GetNearest(agent.transform.position).node;

        // Find a random walkable position

        for (int i = 0; i < threshold; ++i)
        {
            rNode = grid.nodes[Random.Range(0, grid.nodes.Length)];

            if (rNode.Walkable == true)
            {
                break;
            }
        }

        path.destination = (Vector3)rNode.position;
        bb.SetValue("lastTarget", path.destination);
        animator.SetInteger("Moving", 1);
    }

}
