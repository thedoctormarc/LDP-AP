using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Framework;
using Pathfinding;


public class AILogic : AI
{
    public enum AI_State { idle, walk, run, fire, die };
    [SerializeField]
    Collider col;
    public Collider _col() => col;
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
    [HideInInspector]
    public float currentHealth;
    GameObject lastAggro;
    public GameObject _lastAggro() => lastAggro;
    public string _weapon() => currentWeapon;
    Parameters parameters;
    float sendPosTime;

    private void Awake()
    {
        currentState = AI_State.idle;
        animator = gameObject.GetComponent<Animator>();
        bb = gameObject.GetComponent<Blackboard>();
        currentWeapon = weaponSlot.transform.GetChild(0).gameObject.name;
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
        parameters = gameObject.GetComponent<Parameters>();

    }

    void Start()
    {
        
    
    }

    void Update()
    {
        if ((sendPosTime += Time.deltaTime) >= Analytics.instance.positionIntervalSec)
        {
            sendPosTime = 0f;

            Analytics.instance.OnPositionChange(gameObject);
        }
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

        lastAggro = enemy;
        currentState = AI_State.fire;
        bb.SetValue("aggro", true);
        animator.SetBool("Aggro", true);
        RelocateWeapon();

        return true;
    }

    public void DeAggro()
    {
        bb.SetValue("aggro", false);  
        animator.SetBool("Aggro", false);
        RelocateWeapon();

        var AIScripts = gameObject.GetComponents<AI>();
        foreach (AI script in AIScripts)
        {
            script.OnDeAggro();
        }

        lastAggro = null;
    }

    public override void OnDeath()
    {
        parameters.OnDeath();
 
        currentState = AI_State.die;
        animator.SetInteger("Moving", 0);
        animator.SetBool("Dead", true);

        if (IsHuman() == false)
        {
            bb.SetValue("dead", true);
            Vector3 max = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            bb.SetValue("lastTarget", max);

        }

    }

    bool IsHuman() => GetComponent<HumanController>() != null;
 
}
