using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Framework;
using Unity.Simulation.Games;

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
        AIParameters r_aIParameters = receptor.GetComponent<AIParameters>();

        if ((r_aIParameters.currentHealth -= damage) <= 0f)  
        {
            // Die

            var AIScripts = receptor.GetComponents<AI>();
            foreach (AI script in AIScripts)
            {
                script.OnDeath();
            }

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

            // Unity Game Simulation
            /*AIParameters e_aIParameters = emitter.GetComponent<AIParameters>();
            string killCounter = "T" + e_aIParameters._team().ToString() + " kills";
            string deathCounter = "T" + r_aIParameters._team().ToString() + " deaths";
            GameSimManager.Instance.IncrementCounter(killCounter, (long)1);
            GameSimManager.Instance.IncrementCounter(deathCounter, (long)1);*/

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
