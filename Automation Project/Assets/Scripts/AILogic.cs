using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Framework;
using Pathfinding;

public class AILogic : AI
{
    public enum AI_State { idle, walk, run, fire, die };
    [HideInInspector]
    public AI_State currentState;
    string currentWeapon;
    public Dictionary<string, Vector3> weaponOffsets;
    [SerializeField]
    GameObject weaponSlot;
    WeaponParameters weaponParameters;
    public GameObject _weaponSlot() => weaponSlot;
    Animator animator;
    Blackboard bb;
    bool[] aggrodEnemiesIndexes;
    public bool[] _aggrodEnemiesIndexes() => aggrodEnemiesIndexes;
    [HideInInspector]
    public float currentHealth;
    int lastAggro;
    public int _lastAggro() => lastAggro;
    AIPath path;

    void Start()
    {
        currentState = AI_State.idle;
        animator = gameObject.GetComponent<Animator>();
        bb = gameObject.GetComponent<Blackboard>();
        currentWeapon = weaponSlot.transform.GetChild(0).gameObject.name;
        path = gameObject.GetComponent<AIPath>();
        weaponOffsets = new Dictionary<string, Vector3>()
        {
            { "rifle_idle", new Vector3(0.126f, 1.151f, 0.44f) },
            { "rifle_walk", new Vector3(0.126f, 1.151f, 0.44f) },
            { "rifle_run", new Vector3(0.126f, 1.151f, 0.44f) },
            { "rifle_fire", new Vector3(0.097f, 1.4f, 0.44f) },
            { "rifle_die", new Vector3(0f, -10f, 0f) },
            { "shotgun_idle", new Vector3(0.126f, 1.151f, 0.44f) },
            { "shotgun_walk", new Vector3(0.126f, 1.151f, 0.44f) },
            { "shotgun_run", new Vector3(0.126f, 1.351f, 0.44f) },
            { "shotgun_fire", new Vector3(0.097f, 1.4f, 0.44f) },
            { "shotgun_die", new Vector3(0f, -10f, 0f) },
            { "sniper_idle", new Vector3(0.126f, 1.151f, 0.44f) },
            { "sniper_walk", new Vector3(0.126f, 1.151f, 0.44f) },
            { "sniper_run", new Vector3(0.126f, 1.351f, 0.44f) },
            { "sniper_fire", new Vector3(0.097f, 1.4f, 0.44f) },
            { "sniper_die", new Vector3(0f, -10f, 0f) }

        };

        weaponParameters = weaponSlot.transform.GetChild(0).GetComponent<WeaponParameters>();

        aggrodEnemiesIndexes = new bool[PlayerManager.instance.transform.childCount];
        for (int i = 0; i < aggrodEnemiesIndexes.Length; ++i)
        {
            aggrodEnemiesIndexes[i] = false;
        }
    }

    void Update()
    {

    }

    public void RelocateWeapon()
    {
        string locate = currentWeapon + "_" + currentState.ToString();
        Transform weaponTransform = weaponSlot.transform.GetChild(0);
        weaponTransform.position = weaponSlot.transform.position;
        weaponTransform.localPosition += weaponOffsets[locate];
    }

    public bool TriggerAggro(GameObject enemy)
    {
        if(weaponParameters.InRange(gameObject, enemy) == false)
        {
            return false;
        }

        for (int i = 0; i < PlayerManager.instance.transform.childCount; ++i)
        {
            if (PlayerManager.instance.transform.GetChild(i).gameObject == enemy)
            {
                aggrodEnemiesIndexes[i] = true;
                lastAggro = i;
                break;
            }
        }

        currentState = AI_State.fire;
        Debug.Log("AI Detected Enemy!");
        bb.SetValue("aggro", true);
        animator.SetBool("Aggro", true);
      //  animator.SetInteger("Moving", 0);
        RelocateWeapon();

        return true;
    }

    public void DeAggro(int index)
    {
        if (index >= 0 && index < aggrodEnemiesIndexes.Length)
        {
            aggrodEnemiesIndexes[index] = false;
        }

        // for the moment completely de-aggro (from all threats)
        bb.SetValue("aggro", false);  
        animator.SetBool("Aggro", false);
        RelocateWeapon();

        var AIScripts = gameObject.GetComponents<AI>();
        foreach (AI script in AIScripts)
        {
            script.OnDeAggro(PlayerManager.instance.GetChildByIndex(index));
        }
    }

    public override void OnDeath()
    {
        currentState = AI_State.die;
        animator.SetInteger("Moving", 0);
        animator.SetBool("Dead", true);
        bb.SetValue("dead", true);
    }

 
}
