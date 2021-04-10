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

                if (GeometryUtility.TestPlanesAABB(camPlanes, aIPerception.col.bounds)) // My camera detected another AI's mesh
                {

                    foreach (Vector3 targetOffset in raycastTargetOffsets)
                    {
                        RaycastHit hit;
                        Vector3 direction = (aIPerception.col.gameObject.transform.position + targetOffset) - (transform.position + new Vector3(0f, parameters._headPositionOffset(), 0f));

                        Debug.DrawRay(transform.position, transform.TransformDirection(direction), Color.red);
                     
                        if (Physics.Raycast(transform.position, transform.TransformDirection(direction), out hit, Mathf.Infinity))
                        {
                            if(hit.transform.parent.gameObject.CompareTag("Player"))
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
