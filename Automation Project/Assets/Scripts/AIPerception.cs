﻿// Reference: https://docs.unity3d.com/ScriptReference/GeometryUtility.TestPlanesAABB.html

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPerception : AI
{
    [SerializeField]
    Collider col;
    AIParameters parameters;
    float visualTime = 0f;
    float auditiveTime = 0f;
    [SerializeField]
    Vector3[] raycastTargetOffsets;
    AILogic aILogic;
    List<GameObject> audioDetected;

    void Start()
    {
        audioDetected = new List<GameObject>();
        parameters = gameObject.GetComponent<AIParameters>();
        aILogic = gameObject.GetComponent<AILogic>();
    }

    public void VisualDetection()
    {
        if ((visualTime += Time.deltaTime) >= parameters._visualRefreshTime())
        {
            visualTime = 0f;

            for (int i = 0; i < PlayerManager.instance.transform.childCount; ++i)
            {
                GameObject child = PlayerManager.instance.transform.GetChild(i).gameObject;

                if (child.gameObject == gameObject )  
                {
                    continue;
                }

                AIParameters aIParameters = child.GetComponent<AIParameters>();
                if (aIParameters._team() == parameters._team())
                {
                    continue;
                }

                AILogic e_aIlogic = child.GetComponent<AILogic>();
                if (e_aIlogic.currentState == AILogic.AI_State.die)
                {
                    continue;
                }

                AIPerception aIPerception = child.GetComponent<AIPerception>();

                Vector3 waistPosition = (transform.position + transform.up * parameters._waistPositionOffset());
                Vector3 enemyWaistPosition = aIPerception.col.gameObject.transform.position
                    + aIPerception.col.gameObject.transform.up * parameters._headPositionOffset();
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
                        Vector3 destination = aIPerception.col.gameObject.transform.position + targetOffset;
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

                                aILogic.TriggerAggro(hit.transform.parent.gameObject);
                                
                                // TODO: trigger aggro in the other AI
                                return;
                            }
           
                        }
                    }
                }
        
            }
        }
    }

    public void AuditiveDetection()
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
                        AIParameters aIParameters = go.GetComponent<AIParameters>();

                        if (aIParameters._team() != parameters._team())
                        {
                            audioDetected.Add(go);
                        }
                    }
                }
            }
        }
    }

    public bool InLineOfFireWithAI (int index)
    {
        GameObject go = PlayerManager.instance.GetChildByIndex(aILogic._lastAggro());

        foreach (Vector3 targetOffset in raycastTargetOffsets)
        {
            RaycastHit hit;
            Vector3 origin = transform.position + transform.up * parameters._headPositionOffset();
            Vector3 destination = go.GetComponent<AIPerception>().col.gameObject.transform.position + targetOffset;
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

    public bool LOF_FromNodePos(Vector3 nodePos)
    {
        for (int i = 0; i < PlayerManager.instance.transform.childCount; ++i)
        {
            GameObject child = PlayerManager.instance.transform.GetChild(i).gameObject;
            AIPerception aIPerception = child.GetComponent<AIPerception>();

            foreach (Vector3 targetOffset in raycastTargetOffsets)
            {
                RaycastHit hit;
                Vector3 origin = nodePos + transform.up * parameters._headPositionOffset();
                Vector3 destination = aIPerception.col.gameObject.transform.position + targetOffset;
                Vector3 direction = destination - origin;

                if (Physics.Raycast(origin, direction, out hit, Mathf.Infinity))
                {
                    if (hit.transform.parent.gameObject == gameObject )  
                    {
                        continue;
                    }

                    AIParameters aIParameters = child.GetComponent<AIParameters>();
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

    public void OnDrawGizmos ()
    {
        if (transform != null)
        {
            Gizmos.color = (audioDetected.Count > 0) ? Color.green : Color.red;
            Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.5f);
            Gizmos.DrawSphere(transform.position, parameters._audioPerceptionRadius());
        }
    }


    public override void OnDeAggro(GameObject go)
    {
        if (audioDetected.Contains(go))
        {
            audioDetected.Remove(go);
        }
    }

    public override void OnDeath()
    {
        audioDetected.Clear();
        visualTime = 0f;
        auditiveTime = 0f;
    }
}
