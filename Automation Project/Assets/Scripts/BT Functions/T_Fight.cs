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
        // aIPerception.VisualPerception();

        if (bb.GetValue<bool>("dead") || !bb.GetValue<bool>("aggro"))
        {
            EndAction(true);
        }

        if (aIPerception.LostAggroLOF())
        {
            EndAction(true);
        }

        
        if (Aim()) // TODO: recalculate from time to time if lost sight with target
        {
            Fire();
        }
    }


    bool Aim()
    {
        // TODO: only loop enemies, and aim at the highest priority!!! (Priority system)

        for (int i = 0; i < aILogic._aggrodEnemiesIndexes().Length; ++i)
        {

            if(aILogic._aggrodEnemiesIndexes()[i] == false)
            {
                continue;
            }

            // Reference: https://docs.unity3d.com/ScriptReference/Vector3.RotateTowards.html

            GameObject enemy = PlayerManager.instance.transform.GetChild(i).gameObject;
            AIParameters aIParameters = enemy.GetComponent<AIParameters>();

            Vector3 origin = aILogic._weaponSlot().transform.GetChild(0).Find("Weapon Tip Position").position;
            Vector3 destination = enemy.transform.position + enemy.transform.up * aIParameters._headPositionOffset(); // TODO: if shotgun or sniper, aim at the body
            Vector3 targetDir = destination - origin;

            float aimSpeed = AIParameters._aimSpeed() * ((aIPerception.IsAudioDetected(enemy)) ? AIParameters._aimSpeedMultiAaudio() : 1f) * Time.deltaTime;

            Vector3 newAimDir = Vector3.RotateTowards(currentAimDir, targetDir, aimSpeed, 0.0f);
            currentAimDir = newAimDir;

            // TODO: check this does not mess up right and up vectors
            Vector3 newForwadVector = agent.transform.forward;
            newForwadVector.z = newAimDir.z;
            agent.transform.forward = newForwadVector;

            currentAimVector = currentAimDir * targetDir.magnitude;

            if (PlayerManager.instance.debug)
            {
                Debug.DrawRay(origin, currentAimVector, Color.cyan);
            }

            currentDestination = origin + currentAimVector;
            if ((currentDestination - destination).magnitude <= aimThreshold)
            {
                return true;
            }

        }
            
        return false;
    }

    void Fire()
    {
        if ((currentFireTime += Time.deltaTime) >= weaponParameters._fireRate() / 1000f)
        {
            Debug.Log("AI is firing!!");
          //   aILogic._weaponSlot().transform.GetChild(0).GetComponent<AudioSource>().Play(); // TODO: only when human player

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

            // TODO: encapsulate in one function in aiperception
            if (Physics.Raycast(origin, direction.normalized, out hit, Mathf.Infinity))
            {
                // 3. Direct hit to an enemy part
                if (hit.transform.parent.gameObject.CompareTag("Player"))
                {

                    // Debug
                    if (PlayerManager.instance.debug)
                    {
                        Debug.DrawRay(origin, direction.normalized, Color.green);
                        Debug.Log("AI Hit an enemy!!");
                    }

                    if (PlayerManager.instance.DamageAI(weaponParameters.GetDamageAtDistance(direction.magnitude), hit.transform.parent.gameObject, agent.gameObject)) // TODO: dont de-aggro completely, only remove particular threat
                    {
                        EndAction(true);
                    };
                }
                else
                {
                    // Debug
                    Debug.DrawRay(origin, direction, Color.red);
                }
            }
        }
    }
}