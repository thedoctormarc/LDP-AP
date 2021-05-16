using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPerception : AI
{
    AIParameters parameters;
    float visualTime = 0f;
    float auditiveTime = 0f;
    [SerializeField]
    Vector3[] raycastTargetOffsets;
    public Vector3[] _raycastTargetOffsets() => raycastTargetOffsets;
    AILogic aILogic;
    List<GameObject> audioDetected;
    public List<GameObject> _audioDetected() => audioDetected;
    List<GameObject> visuallyDetected;
    public List<GameObject> _visuallyDetected() => visuallyDetected;
    void Start()
    {
        audioDetected = new List<GameObject>();
        visuallyDetected = new List<GameObject>();
        parameters = gameObject.GetComponent<AIParameters>();
        aILogic = gameObject.GetComponent<AILogic>();
    }

    public void VisualDetection(bool detectPickups = false, bool detectAllies = false)
    {

        if ((visualTime += Time.deltaTime) >= parameters._visualRefreshTime())
        {
            visualTime = 0f;
            visuallyDetected.Clear();

            // Detect enemies
            for (int i = 0; i < PlayerManager.instance.transform.childCount; ++i)
            {
                GameObject child = PlayerManager.instance.transform.GetChild(i).gameObject;

                if (child.gameObject == gameObject )  
                {
                    continue;
                }

                Parameters aIParameters = child.GetComponent<Parameters>();
                if (aIParameters._team() == parameters._team() && detectAllies == false)
                {
                    continue;
                }

                AILogic e_aIlogic = child.GetComponent<AILogic>();
                if (e_aIlogic.currentState == AILogic.AI_State.die)
                {
                    continue;
                }

                AILogic e_aILogic = child.GetComponent<AILogic>();

                Vector3 waistPosition = (transform.position + transform.up * parameters._waistPositionOffset());
                Vector3 enemyWaistPosition = e_aILogic._col().gameObject.transform.position
                    + e_aILogic._col().gameObject.transform.up * parameters._headPositionOffset();
                Vector3 dirToEnemy = enemyWaistPosition - waistPosition;

                // Debug
                if (PlayerManager.instance.debug)
                {
                    Debug.DrawRay(waistPosition, dirToEnemy, Color.blue);
                    Debug.DrawRay(waistPosition, transform.forward, Color.blue);
                }

                // 2. Enemy within max view angle
                float horizontalAngle = Vector3.Angle(transform.forward, dirToEnemy);
                if (horizontalAngle <= parameters._maxViewAngle() / 2f)
                {
                    foreach (Vector3 targetOffset in raycastTargetOffsets)
                    {
                        RaycastHit hit;
                        Vector3 origin = transform.position + transform.up * parameters._headPositionOffset();
                        Vector3 destination = e_aILogic._col().gameObject.transform.position + targetOffset;
                        Vector3 direction = destination - origin;

                        if (Physics.Raycast(origin, direction, out hit, Mathf.Infinity))
                        {
                            if (hit.transform.parent.gameObject == gameObject) 
                            {
                                continue;
                            }

                            if (hit.transform.parent.gameObject.CompareTag("Player"))
                            {
                                if (PlayerManager.instance.debug)
                                {
                                    Debug.DrawRay(origin, direction, Color.green);
                                    Debug.Log("AI named " + gameObject.name + "will begin to fire AI named " + hit.transform.parent.gameObject.name);
                                }


                                // Add to visually detected, but only aggro other teams!
                                visuallyDetected.Add(hit.transform.parent.gameObject);

                                if (hit.transform.parent.gameObject.GetComponent<Parameters>()._team() != parameters._team())
                                {
                                    aILogic.TriggerAggro(hit.transform.parent.gameObject);
                                }

                            }
           
                        }
                    }
                }
        
            }


            // Detect pickups 
            if (detectPickups)
            {
                for (int i = 0; i < AppManager.instance._pickups().transform.childCount; ++i)
                {
                    GameObject pickup = AppManager.instance._pickups().transform.GetChild(i).gameObject;
                    Vector3 origin = transform.position + transform.up * parameters._headPositionOffset();
                    Vector3 dir  = pickup.transform.position - origin;
                    float horizontalAngle = Vector3.Angle(transform.forward, dir);

                    if (horizontalAngle <= parameters._maxViewAngle() / 2f)
                    {
                        if (pickup.GetComponent<Pickup>().Active() == false)
                        {
                            continue;
                        }

                        if (LOF_ToObjectPos(pickup))
                        {
                            visuallyDetected.Add(pickup);
                        }
                    }

                }
            }
        }

    }

    public void AuditiveDetection(bool detectAllies = false)
    {
        if ((auditiveTime += Time.deltaTime) >= parameters._auditiveRefreshTime())
        {
            auditiveTime = 0f;
            audioDetected.Clear();

            // Search new enemies inside radius
            var colliders = Physics.OverlapSphere(transform.position, parameters._audioPerceptionRadius(), 1<<9); // AI layer

            foreach (Collider col in colliders)
            {
                GameObject go = col.transform.parent.gameObject;

                if (go.CompareTag("Player"))
                {
                    if (go != gameObject)
                    {
                        Parameters aIParameters = go.GetComponent<Parameters>();

                        if (aIParameters._team() != parameters._team() || detectAllies)
                        {
                            Animator animator = go.GetComponent<Animator>();

                            if (animator.GetInteger("Moving") == 2)
                            {
                                audioDetected.Add(go);
                            }
                        }
                    }
                }
            }
        }
    }

    public bool IsAudioDetected (GameObject enemy)
    {
        return audioDetected.Contains(enemy);
    }

    // To an enemy 
    public bool InLineOfFireWithAI (GameObject go)
    {
        foreach (Vector3 targetOffset in raycastTargetOffsets)
        {
            RaycastHit hit;
            Vector3 origin = transform.position + transform.up * parameters._headPositionOffset();
            Vector3 destination = go.GetComponent<AILogic>()._col().gameObject.transform.position + targetOffset;
            Vector3 direction = destination - origin;

            if (Physics.Raycast(origin, direction, out hit, Mathf.Infinity))
            {
                if (hit.transform.parent.gameObject == go)
                {
                    return true;
                }

            }
        }
        return false;
    }

    // Enemy visually detected at all
    public bool EnemyVisuallyDetected(GameObject enemy) => LOF_FromNodePos(transform.position, enemy) != 0 && EnemyWithinViewAngle(enemy);

    // Enemy within horizontal view Angle
    public bool EnemyWithinViewAngle (GameObject enemy)
    {
        AILogic aiLogic = enemy.GetComponent<AILogic>();
        Vector3 waistPosition = (transform.position + transform.up * parameters._waistPositionOffset());
        Vector3 enemyWaistPosition = aiLogic._col().gameObject.transform.position
            + aiLogic._col().gameObject.transform.up * parameters._headPositionOffset();
        Vector3 dirToEnemy = enemyWaistPosition - waistPosition;

        float horizontalAngle = Vector3.Angle(transform.forward, dirToEnemy);
        if (horizontalAngle <= parameters._maxViewAngle() / 2f)
        {
            return true;
        }
        return false;
    }


    // To all enemies from postion
    public bool LOF_FromNodePos(Vector3 nodePos)
    {
        for (int i = 0; i < PlayerManager.instance.transform.childCount; ++i)
        {
            GameObject child = PlayerManager.instance.transform.GetChild(i).gameObject;
            AILogic aILogic = child.GetComponent<AILogic>();

            foreach (Vector3 targetOffset in raycastTargetOffsets)
            {
                RaycastHit hit;
                Vector3 origin = nodePos + transform.up * parameters._headPositionOffset();
                Vector3 destination = aILogic._col().gameObject.transform.position + targetOffset;
                Vector3 direction = destination - origin;

                if (Physics.Raycast(origin, direction, out hit, Mathf.Infinity))
                {
                    if (hit.transform.parent.gameObject == gameObject )  
                    {
                        continue;
                    }

                    Parameters aIParameters = child.GetComponent<Parameters>();
                    if (aIParameters._team() == parameters._team())
                    {
                        continue;
                    }

                    if (hit.transform.parent.gameObject.CompareTag("Player"))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }


    // To an enemy from position
    public int LOF_FromNodePos(Vector3 nodePos, GameObject enemy)
    {
        int ret = 0;
        AILogic aILogic = enemy.GetComponent<AILogic>();

        foreach (Vector3 targetOffset in raycastTargetOffsets)
        {
            RaycastHit hit;
            Vector3 origin = nodePos + transform.up * parameters._headPositionOffset();
            Vector3 destination = aILogic._col().gameObject.transform.position + targetOffset;
            Vector3 direction = destination - origin;

            if (Physics.Raycast(origin, direction, out hit, Mathf.Infinity))
            {
                if (hit.transform.parent.gameObject == enemy)
                {
                    ++ret;
                }
            }
        }

        return ret;
    }

   /* public void OnDrawGizmos ()
    {
        if (transform != null && audioDetected != null)
        {
            Gizmos.color = (audioDetected.Count > 0) ? Color.green : Color.red;
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.5f);
            Gizmos.DrawSphere(transform.position, parameters._audioPerceptionRadius());
        }
    }*/

    public bool LOF_ToObjectPos (GameObject go)
    {
        RaycastHit hit;
        Vector3 origin = gameObject.transform.position + transform.up * parameters._headPositionOffset();
        Vector3 direction = go.transform.position - origin;

        if (Physics.Raycast(origin, direction, out hit, Mathf.Infinity))
        {
            if (hit.transform.gameObject == go)
            {
                return true;
            }
        }

        return false;
    }


    public override void OnDeAggro()
    {
        audioDetected.Remove(aILogic._lastAggro());
    }

    public override void OnDeath()
    {
        audioDetected.Clear();
        visualTime = 0f;
        auditiveTime = 0f;
    }

    public bool LostAggroLOF()
    {
 
        if (aILogic._lastAggro() == null || InLineOfFireWithAI(aILogic._lastAggro()) == false)
        {
            aILogic.DeAggro();
            return true;
        }

        return false;
    }
}
