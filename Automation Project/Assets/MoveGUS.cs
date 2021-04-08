using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class MoveGUS : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()

    {
        transform.position += new Vector3(Time.deltaTime, 0, 0);
        GetComponent<GraphUpdateScene>().Apply();
    }
}
