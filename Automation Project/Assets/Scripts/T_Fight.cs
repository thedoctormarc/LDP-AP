using NodeCanvas.Framework;
using UnityEngine;
using Pathfinding;

public class T_Fight : ActionTask
{
    AIPath path;
    AIPerception aIPerception;
    AILogic aILogic;
    AIParameters AIParameters;
    WeaponParameters weaponParameters;
    float currentFireTime = 0f;
    Vector3 currentAimDir;

    protected override string OnInit()
    {
        currentAimDir = agent.transform.forward;
        path = agent.gameObject.GetComponent<AIPath>();
        path.canMove = false;
        path.canSearch = false;
        aIPerception = agent.gameObject.GetComponent<AIPerception>();
        AIParameters = agent.gameObject.GetComponent<AIParameters>();
        aILogic = agent.gameObject.GetComponent<AILogic>();
        weaponParameters = aILogic._weaponSlot().transform.GetChild(0).GetComponent<WeaponParameters>();

        return null;
    }

    protected override void OnUpdate()  
    {
        // aIPerception.VisualPerception();
        
        if(Aim()) // TODO: recalculate from time to time if lost sight with target
        {
            Fire();
        }
    }

    bool Aim()
    {
        // TODO: only loop enemies, and aim at the highest priority!!! (Priority system)

        for (int i = 0; i < aILogic._aggrodEnemiesIndixes().Length; ++i)
        {

            if(aILogic._aggrodEnemiesIndixes()[i] == false)
            {
                continue;
            }

            // Reference: https://docs.unity3d.com/ScriptReference/Vector3.RotateTowards.html

            GameObject enemy = PlayerManager.instance.transform.GetChild(i).gameObject;
            AIParameters aIParameters = enemy.GetComponent<AIParameters>();

            Vector3 origin = aILogic._weaponSlot().transform.GetChild(0).Find("Weapon Tip Position").position;
            Vector3 destination = enemy.transform.position + enemy.transform.up * aIParameters._headPositionOffset(); // TODO: if shotgun or sniper, aim at the body
            Vector3 targetDir = destination - origin;
       
            float aimSpeed = AIParameters._aimSpeed() * Time.deltaTime;

            Vector3 newAimDir = Vector3.RotateTowards(currentAimDir, targetDir, aimSpeed, 0.0f);
            currentAimDir = newAimDir;

            // TODO: check this does not mess up right and up vectors
            Vector3 newForwadVector = agent.transform.forward;
            newForwadVector.z = newAimDir.z;
            agent.transform.forward = newForwadVector;

            Vector3 longAimVector = currentAimDir * 100f;
            Debug.DrawRay(origin, longAimVector, Color.cyan);

        }
            
        return false;
    }

    void Fire()
    {
        float frameFreq = 1f / Time.deltaTime;
        float framesUntilBullet = frameFreq / weaponParameters._roundsPerSec();
        float secondsUntilBullet = framesUntilBullet * Time.deltaTime;

        if((currentFireTime += Time.deltaTime) >= secondsUntilBullet)
        {
            currentFireTime = 0f;
        }

    }
}