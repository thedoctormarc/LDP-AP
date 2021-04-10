// Reference: https://docs.unity3d.com/ScriptReference/GeometryUtility.TestPlanesAABB.html

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Framework;

public class AIPerception : MonoBehaviour
{
    [SerializeField]
    Collider col;
    AIParameters parameters;
    [SerializeField]
    Camera cam;
    Plane[] camPlanes;
    Blackboard bb;
    float currentTime = 0f;
    [SerializeField]
    Vector3[] raycastTargetOffsets;

    void Start()
    {
        parameters = gameObject.GetComponent<AIParameters>();
        camPlanes = GeometryUtility.CalculateFrustumPlanes(cam);
        bb = gameObject.GetComponent<Blackboard>();
    }
 
    public void VisualPerception()
    {
        if ((currentTime += Time.deltaTime) >= parameters._visualRefreshTime())
        {
            currentTime = 0f;

            for (int i = 0; i < PlayerManager.instance.transform.childCount; ++i)
            {
                if (PlayerManager.instance.transform.GetChild(i).gameObject == gameObject) // TODO: if not enemy, ignore!
                {
                    continue;
                }

                AIPerception aIPerception = PlayerManager.instance.transform.GetChild(i).GetComponent<AIPerception>();

                // 1. Enemy Collider inside camera bounds
                if (GeometryUtility.TestPlanesAABB(camPlanes, aIPerception.col.bounds)) 
                {
                    Vector3 waistPosition = (transform.position + transform.up * parameters._waistPositionOffset());
                    Vector3 enemyWaistPosition = aIPerception.col.gameObject.transform.position 
                        + aIPerception.col.gameObject.transform.up * parameters._headPositionOffset();
                    Vector3 dirToEnemy = enemyWaistPosition - waistPosition;

                    // Debug
                    Debug.DrawRay(waistPosition, transform.TransformDirection(dirToEnemy), Color.blue);
                    Debug.DrawRay(waistPosition, transform.forward, Color.blue);


                    // 2. Enemy within max view angle
                    float horizontalAngle = Vector3.Angle(transform.forward, dirToEnemy);
                    if (horizontalAngle <= parameters._maxViewAngle())
                    {
                        foreach (Vector3 targetOffset in raycastTargetOffsets)
                        {
                            RaycastHit hit;
                            Vector3 origin = transform.position + transform.up * parameters._headPositionOffset();
                            Vector3 destination = aIPerception.col.gameObject.transform.position + targetOffset;
                            Vector3 direction = destination - origin;

                            // Debug
                            Debug.DrawRay(origin, transform.TransformDirection(direction), Color.red);

                            if (Physics.Raycast(origin, direction, out hit, Mathf.Infinity))
                            {
                                // 3. Direct hit to an enemy part
                                if (hit.transform.parent.gameObject.CompareTag("Player"))
                                {
                                    Debug.Log("AI Detected Enemy!");
                                    bb.SetValue("aggro", true);

                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
