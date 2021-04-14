using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Framework;
public class PlayerManager : MonoBehaviour
{
     
    public bool debug;

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
       if (Input.GetKeyDown(KeyCode.F1) == true)
        {
            debug = !debug;
        }

       if (Input.GetKeyDown(KeyCode.RightArrow) == true)
        {

        }
    }

    public bool DamageAI(float damage, GameObject receptor, GameObject emitter) // de-aggro killer, killed and all other players that were aggro-ing the killed
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

            for (int i = 0; i < transform.childCount; ++i) // instantly de-aggro all AIs from the killer. TODO: de-aggro but not completely if more threats 
            {
                GameObject go = transform.GetChild(i).gameObject;
                if (go == receptor || go == emitter)
                {
                    continue;
                }

                go.GetComponent<AILogic>().DeAggro(GetChildIndex(receptor));
            }

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

    public GameObject GetChildByIndex(int index)
    {
        return transform.GetChild(index).gameObject;
    }

}
