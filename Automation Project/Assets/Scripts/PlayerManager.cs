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
            aIParameters.currentHealth = 0f;
            receptor.GetComponent<AILogic>().currentState = AILogic.AI_State.die;
            Animator animator = receptor.GetComponent<Animator>();
            animator.SetBool("Moving", false);
            animator.SetBool("Aggro", false);
            animator.SetBool("Dead", true);
            Blackboard bb = receptor.GetComponent<Blackboard>();
            bb.SetValue("dead", true);

            // Both De-aggro (TODO: only works 1 vs 1)
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
