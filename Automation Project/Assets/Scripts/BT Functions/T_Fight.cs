using NodeCanvas.Framework;
using UnityEngine;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;

public class T_Fight : ActionTask
{
    AIPath path;
    AIPerception aIPerception;
    AILogic aILogic;
    AIParameters AIParameters;
    WeaponParameters weaponParameters;
    float currentFireTime = 0f;
    Vector3 currentAimDir;
    Vector3 currentAimVector;
    Vector3 currentDestination;
    float aimThreshold = 0.05f;
    Blackboard bb;
    Dictionary<int, float> deAggroCurrentTime;
    Animator animator;

    protected override string OnInit()
    {
        deAggroCurrentTime = new Dictionary<int, float>();
        path = agent.gameObject.GetComponent<AIPath>();
        animator = agent.gameObject.GetComponent<Animator>();
        bb = agent.gameObject.GetComponent<Blackboard>();
        aIPerception = agent.gameObject.GetComponent<AIPerception>();
        AIParameters = agent.gameObject.GetComponent<AIParameters>();
        aILogic = agent.gameObject.GetComponent<AILogic>();
        weaponParameters = aILogic._weaponSlot().transform.GetChild(0).GetComponent<WeaponParameters>();

        return null;
    }

    protected override void OnExecute()
    {
        path.canMove = false;
        path.canSearch = false;
        aILogic.currentState = AILogic.AI_State.fire;
        currentAimDir = agent.transform.forward;
        animator.SetInteger("Moving", 0);
    }

    protected override void OnUpdate()  
    {

        if (bb.GetValue<bool>("dead") || !bb.GetValue<bool>("aggro"))
        {
            EndAction(true);
            return;
        }

        if (aIPerception.LostAggroLOF())
        {
            EndAction(true);
            return;
        }

        
        if (Aim())  
        {
            Fire();
        }
    }


    bool Aim()
    {
        GameObject enemy = aILogic._lastAggro();
        AIParameters aIParameters = enemy.GetComponent<AIParameters>();
        GameObject weapon = aILogic._weaponSlot().transform.GetChild(0).gameObject;

        Vector3 origin = weapon.transform.Find("Weapon Tip Position").position;
        Vector3 destination = enemy.transform.position + enemy.transform.up * aIParameters._headPositionOffset();
        Vector3 targetDir = destination - origin;

        float aimSpeed = AIParameters._aimSpeed() *
            ((aIPerception.IsAudioDetected(enemy)) ? AIParameters._aimSpeedMultiAudio() : 1f)
            * weapon.GetComponent<WeaponParameters>().GetAimMulti()
            * Time.deltaTime;

        Vector3 newAimDir = Vector3.RotateTowards(currentAimDir, targetDir, aimSpeed, 0.0f);
        currentAimDir = newAimDir;

        Vector3 newForwadVector = agent.transform.forward;
        newForwadVector.z = newAimDir.z;
        agent.transform.rotation = Quaternion.LookRotation(newForwadVector);

        currentAimVector = currentAimDir * targetDir.magnitude;

        currentDestination = origin + currentAimVector;
        if ((currentDestination - destination).magnitude <= aimThreshold)
        {
            return true;
        }

        return false;
    }

    void Fire()
    {
        if ((currentFireTime += Time.deltaTime) >= weaponParameters._fireRate() / 100f)
        {
            currentFireTime = 0f;

            float signedAimSpread = AIParameters._aimSpread() / 2f;
            float xOffset = Random.Range(-signedAimSpread, signedAimSpread);
            float yOffset = Random.Range(-signedAimSpread, signedAimSpread);
            float zOffset = Random.Range(-signedAimSpread, signedAimSpread);

            Vector3 offset = new Vector3(xOffset, yOffset, zOffset);

            RaycastHit hit;
            Vector3 bulletDestination = currentDestination + offset;
            Vector3 origin = aILogic._weaponSlot().transform.GetChild(0).Find("Weapon Tip Position").position;
            Vector3 direction = bulletDestination - origin;

            if (Physics.Raycast(origin, direction.normalized, out hit, Mathf.Infinity))
            {

                if (hit.transform.parent.gameObject.CompareTag("Player"))
                {

                    AIParameters aIParameters = hit.transform.parent.gameObject.GetComponent<AIParameters>();
                    if (aIParameters._team() == AIParameters._team())
                    {
                        return;
                    }

                    if (PlayerManager.instance.DamageAI(weaponParameters.GetDamageAtDistance(direction.magnitude), hit.transform.parent.gameObject, agent.gameObject))  
                    {
                        EndAction(true);
                    };
                }
            }
        }
    }
}