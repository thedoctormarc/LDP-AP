using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.Framework;

public class AILogic : MonoBehaviour
{
    public enum AI_State { idle, walk, fire, die};
    public AI_State currentState;
    string currentWeapon;
    public Dictionary<string, Vector3> weaponOffsets;
    [SerializeField]
    GameObject weaponSlot;
    public GameObject _weaponSlot() => weaponSlot;
    Animator animator;
    Blackboard bb;
    bool[] aggrodEnemiesIndexes;
    public bool[] _aggrodEnemiesIndexes() => aggrodEnemiesIndexes;

    public float currentHealth;

    void Start()
    {
        currentState = AI_State.idle;
        animator = gameObject.GetComponent<Animator>();
        bb = gameObject.GetComponent<Blackboard>();
        currentWeapon = "rifle";
        weaponOffsets = new Dictionary<string, Vector3>()
        {
            { "rifle_idle", new Vector3(0.126f, 1.151f, 0.44f) },
            { "rifle_fire", new Vector3(0.097f, 1.4f, 0.44f) }

        };

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

    public void SwitchWeapon(string newWeapon)
    {

    }

    public void TriggerAggro(GameObject enemy)
    {
        for (int i = 0; i < PlayerManager.instance.transform.childCount; ++i)
        {
            if(PlayerManager.instance.transform.GetChild(i).gameObject == enemy)
            {
                aggrodEnemiesIndexes[i] = true;
                break;
            }
        }

        currentState = AI_State.fire;
        Debug.Log("AI Detected Enemy!");
        bb.SetValue("aggro", true);
        animator.SetBool("Aggro", true);
        animator.SetBool("Moving", false);
        RelocateWeapon();
    }

    public void DeAggro(int index)
    {
        if (index >= 0 && index < aggrodEnemiesIndexes.Length)
        {
            aggrodEnemiesIndexes[index] = false;
        }

        bb.SetValue("aggro", false); // for the moment completely de-aggro
    }

}
