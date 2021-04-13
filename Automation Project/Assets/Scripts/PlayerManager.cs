using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Framework;
public class PlayerManager : MonoBehaviour
{
    // Start is called before the first frame update

    public static PlayerManager instance;

    private void Awake()
    {
        instance = this;
    //    DontDestroyOnLoad(this);
    }

    void Start()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool DamageAI(float damage, GameObject receptor, GameObject emitter)
    {
        AIParameters aIParameters = receptor.GetComponent<AIParameters>();

        if ((aIParameters.currentHealth -= damage) <= 0f)  
        {
            // Die
            aIParameters.currentHealth = 0f;
            receptor.GetComponent<AILogic>().currentState = AILogic.AI_State.die;
            Animator rAnimator = receptor.GetComponent<Animator>();
            rAnimator.SetBool("Moving", false);
            rAnimator.SetBool("Dead", true);
            Blackboard bb = receptor.GetComponent<Blackboard>();
            bb.SetValue("dead", true);

            // de-aggro both
            receptor.GetComponent<AILogic>().DeAggro(GetChildIndex(emitter));
            emitter.GetComponent<AILogic>().DeAggro(GetChildIndex(receptor));

            return true;
        }

        return false;
    }

    public int GetChildIndex(GameObject go)
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            if(transform.GetChild(i).gameObject == go)
            {
                return i;
            }
        }

        return int.MaxValue;
    }
}
