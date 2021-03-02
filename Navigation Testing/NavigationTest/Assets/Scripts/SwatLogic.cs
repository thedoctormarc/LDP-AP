using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwatLogic : MonoBehaviour
{
    Pathfinding.AIPath path;
    GameObject swat;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        path = GetComponent<Pathfinding.AIPath>();
        swat = transform.GetChild(0).gameObject;
        animator = swat.GetComponent<Animator>();

        StartMoving();
    }

    void StartMoving()
    {
        path.canSearch = true;
        path.canMove = true;
        animator.SetBool("Moving", true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
