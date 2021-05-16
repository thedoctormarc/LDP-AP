using NodeCanvas.Framework;
using UnityEngine;

public class EndNode : ActionTask
{
    // Start is called before the first frame update
    protected override string OnInit()
    {
        EndAction(true);
        return  null;
    }

    protected override void OnExecute()
    {
        EndAction(true);
    }
}
