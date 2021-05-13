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

    }

    public bool DamageAI(float damage, GameObject receptor, GameObject emitter) // de-aggro killer, killed and all other players that were aggro-ing the killed
    {
        Parameters r_aIParameters = receptor.GetComponent<Parameters>();

        r_aIParameters.UpdateHealth(r_aIParameters._currentHealth() - damage);

        if (r_aIParameters ._currentHealth() <= 0f)  
        {
            // Die

            var AIScripts = receptor.GetComponents<AI>();
            foreach (AI script in AIScripts)
            {
                script.OnDeath();
            }

            // de-aggro both
            if (IsHuman(receptor) == false)
            {
                receptor.GetComponent<AILogic>().DeAggro();
            }

            if (IsHuman(emitter) == false)
            {
                emitter.GetComponent<AILogic>().DeAggro();
            }

            for (int i = 0; i < transform.childCount; ++i) // instantly de-aggro all AIs from the killer. TODO: de-aggro but not completely if more threats 
            {
                GameObject go = transform.GetChild(i).gameObject;
                if (go == receptor || go == emitter)
                {
                    continue;
                }

                if (IsHuman(go) == false)
                {
                    go.GetComponent<AILogic>().DeAggro();
                }
            }

            // Unity Game Simulation
            Parameters e_aIParameters = emitter.GetComponent<Parameters>();
            string killCounter = "T" + e_aIParameters._team().ToString() + " kills";
            string deathCounter = "T" + r_aIParameters._team().ToString() + " deaths";
            GameSimManager.Instance.IncrementCounter(killCounter, (long)1);
            GameSimManager.Instance.IncrementCounter(deathCounter, (long)1);

            return true;
        }

        return false;
    }

    bool IsHuman (GameObject go)
    {
        return go.GetComponent<HumanController>() != null;
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
