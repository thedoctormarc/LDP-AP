// Reference: https://docs.unity3d.com/ScriptReference/GeometryUtility.TestPlanesAABB.html

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPerception : MonoBehaviour
{
    [SerializeField]
    Collider col;
    AIParameters parameters;
    [SerializeField]
    Camera cam;
    Plane[] camPlanes;
    float currentTime = 0f;
    [SerializeField]
    Vector3[] raycastTargetOffsets;
    AILogic aILogic;

    void Start()
    {
        parameters = gameObject.GetComponent<AIParameters>();
        camPlanes = GeometryUtility.CalculateFrustumPlanes(cam);
        aILogic = gameObject.GetComponent<AILogic>();
    }
 
    public void VisualPerception()
    {
        if ((currentTime += Time.deltaTime) >= parameters._visualRefreshTime())
        {
            currentTime = 0f;

            for (int i = 0; i < PlayerManager.instance.transform.childCount; ++i)
            {
                GameObject child = PlayerManager.instance.transform.GetChild(i).gameObject;
                if (child.gameObject == gameObject) // TODO: if not enemy, ignore!
                {
                    continue;
                }

                // 0. Check enemy not dead (TODO: not spawning either)
                if((child.GetComponent<AILogic>().currentState == AILogic.AI_State.die))
                {
                    continue;
                }

                AIPerception aIPerception = child.GetComponent<AIPerception>();

                // 1. Enemy Collider inside camera bounds
              /*  if (GeometryUtility.TestPlanesAABB(camPlanes, aIPerception.col.bounds)) // bugs???
                {*/
                    Vector3 waistPosition = (transform.position + transform.up * parameters._waistPositionOffset());
                    Vector3 enemyWaistPosition = aIPerception.col.gameObject.transform.position 
                        + aIPerception.col.gameObject.transform.up * parameters._headPositionOffset();
                    Vector3 dirToEnemy = enemyWaistPosition - waistPosition;

                    // Debug
                    Debug.DrawRay(waistPosition, dirToEnemy, Color.blue);
                    Debug.DrawRay(waistPosition, transform.forward, Color.blue);


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
                                if (hit.transform.parent.gameObject == gameObject) // TODO: if not enemy, ignore!
                                {
                                    continue;
                                }

                                // 3. Direct hit to an enemy part
                                if (hit.transform.parent.gameObject.CompareTag("Player"))
                                {
                                    // Debug
                                    Debug.DrawRay(origin, direction, Color.green);

                                    Debug.Log("AI named " + gameObject.name + "will begin to fire AI named " + hit.transform.parent.gameObject.name);

                                    aILogic.TriggerAggro(hit.transform.parent.gameObject);

                                    return;
                                }
                          /*      else
                                {
                                    // Debug
                                    Debug.DrawRay(origin, direction, Color.red);
                                }*/
                            }
                        }
                    }
          //      }
            }
        }
    }
}
