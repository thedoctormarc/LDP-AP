using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrioritizeTask : MonoBehaviour
{
    public enum taskType { NO_TYPE, HEALTH, POINTS, TEAMMATE, FIGHT}
    taskType[] priorities;
    AILogic.Player_Type pType;
    AIPerception aIPerception;
    Vector3 currentTarget;
    int currentPriority;

    // Start is called before the first frame update
    void Start()
    {
        currentTarget = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        currentPriority = int.MaxValue;
        pType = gameObject.GetComponent<AILogic>().pType;
        aIPerception = gameObject.GetComponent<AIPerception>();

        switch (pType)
        {
            case AILogic.Player_Type.collector:
                {
                    priorities[0] = taskType.POINTS;
                    priorities[1] = taskType.HEALTH;
                    priorities[2] = taskType.FIGHT;
                    priorities[3] = taskType.TEAMMATE;
                    break;
                }

            case AILogic.Player_Type.killer:
                {
                    priorities[0] = taskType.FIGHT;
                    priorities[1] = taskType.HEALTH;
                    priorities[2] = taskType.TEAMMATE;
                    priorities[3] = taskType.POINTS;
                    break;
                }


            case AILogic.Player_Type.socializer:
                {
                    priorities[0] = taskType.TEAMMATE;
                    priorities[1] = taskType.HEALTH;
                    priorities[2] = taskType.FIGHT;
                    priorities[3] = taskType.POINTS;
                    break;
                }
        }


    }

    // Update is called once per frame
    void Update()
    {
       
    }

    void Detect()
    {
        aIPerception.VisualDetection(true, true);
        aIPerception.AuditiveDetection();

        List<GameObject> visuallyDetected = aIPerception._visuallyDetected();
        List<GameObject> audioDetected = aIPerception._audioDetected();

        ScanForTarget(visuallyDetected);
        ScanForTarget(audioDetected);
    }

    void ScanForTarget(List<GameObject> detected)
    {
        if (detected.Count == 0)
        {
            return;
        }

        Vector3 newTarget = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        int newPriority = int.MaxValue;
        taskType newTaskType = taskType.NO_TYPE;

        for (int i = 0; i < detected.Count; ++i)
        {
            GameObject go = detected[i];
            int go_priority = int.MaxValue;
            taskType go_TaskType = taskType.NO_TYPE;

            if (go.CompareTag("pickup"))
            {
                go_TaskType = (taskType)System.Enum.Parse(typeof(taskType), go.GetComponent<Pickup>().pickupType.ToString());
            }
            else if (go.transform.parent.gameObject.CompareTag("Player"))
            {
                // TODO
            }

            int lastPriority = newPriority;
            newPriority = ((go_priority = GetPriority(newTaskType)) < newPriority) ? go_priority : newPriority;

            if (newPriority != lastPriority)
            {
                newTaskType = go_TaskType;
                newTarget = go.transform.position;
            }
        }


    }

    int GetPriority (taskType type)
    {
        for (int i = 0; i < priorities.Length; ++i)
        {
            if (priorities[i] == type)
            {
                return (int)priorities[i];
            }
        }
        return int.MaxValue;
    }

}
