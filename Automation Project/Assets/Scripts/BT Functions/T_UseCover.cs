using NodeCanvas.Framework;
using UnityEngine;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;

public class T_UseCover : ActionTask
{
    // Start is called before the first frame update
    protected override string OnInit()
    {
        EndAction(true);
        return null;
    }

    protected override void OnExecute()
    {
        EndAction(true);
    }
}
