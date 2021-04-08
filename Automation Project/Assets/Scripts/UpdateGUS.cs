using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class UpdateGUS : MonoBehaviour
{
    GraphUpdateScene GUS;

    void Start()
    {
        GUS = GetComponent<GraphUpdateScene>();
    }

    // Update is called once per frame
    void Update()
    {
        GUS.Apply();
    }
}
