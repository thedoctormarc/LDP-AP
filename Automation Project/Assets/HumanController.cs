using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class HumanController : MonoBehaviour
{
    [SerializeField]
    float aimSpeed = 3f;
    private float xRot = 0.0f;
    private float yRot = 0.0f;
    private Camera camera;
    private CharacterController controller;
    private WeaponParameters wParameters;
    private Parameters parameters;
    private float currentFireTime = 0f;
    private AILogic aILogic;
    private AudioSource aS;
    private float currentVel = 0f;
    private float currentRespawnTime = 0f;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        camera = transform.Find("Line Of Sight").GetComponent<Camera>();
        controller = GetComponent<CharacterController>();
        parameters = GetComponent<Parameters>();
        aILogic = GetComponent<AILogic>();
        aS = transform.Find("Weapon Slot").GetChild(0).GetComponent<AudioSource>();
        wParameters = aILogic._weaponSlot().transform.GetChild(0).GetComponent<WeaponParameters>();
    }

    void Update()
    {
        Debug.DrawRay(camera.transform.position, camera.transform.forward * 100f, Color.white);

        switch(aILogic.currentState)
        {
            case AILogic.AI_State.die:
            {
                    if ((currentRespawnTime += Time.deltaTime) >= parameters._respawnTime())
                    {
                        currentRespawnTime = 0f;
                        Respawn();
                    }
                    return; 
            }
        }

        MovementInput();
        AimInput();
        FireInput();
    }


    void MovementInput()
    {
        float x = Input.GetAxis("Horizontal") * parameters._walkSpeed();
        float y = Input.GetAxis("Vertical") * parameters._walkSpeed();
        controller.Move((transform.right * x + transform.forward * y) * Time.deltaTime);

        // Gravity
        if (controller.isGrounded)
        {
            currentVel = 0;
        }
        else
        {
            currentVel -= Physics.gravity.magnitude * Time.deltaTime;
            controller.Move(new Vector3(0, currentVel, 0));
        }
    }

    void AimInput()
    {
        // Aim
        float x = Input.GetAxis("Mouse X") * aimSpeed;
        float y = Input.GetAxis("Mouse Y") * aimSpeed;

        yRot += x;
        xRot -= y;
        xRot = Mathf.Clamp(xRot, -90, 90);

        camera.transform.eulerAngles = new Vector3(xRot, yRot, 0.0f);
        transform.eulerAngles = new Vector3(0.0f, yRot, 0.0f);


    }

    void FireInput()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            if ((currentFireTime += Time.deltaTime) >= wParameters._fireRate() / 100f)
            {
                currentFireTime = 0f;
                aS.Play();
                RaycastHit hit;
                bool collide = Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, Mathf.Infinity);
        

                if (collide)
                {

                    if (hit.transform.parent.gameObject.CompareTag("Player"))
                    {

                        Parameters aIParameters = hit.transform.parent.gameObject.GetComponent<Parameters>();
                        if (aIParameters._team() == parameters._team())
                        {
                            return;
                        }

                        PlayerManager.instance.DamageAI(wParameters.GetDamageAtDistance((hit.point - transform.position).magnitude), hit.transform.parent.gameObject, gameObject);
                    }
                }
            }
        }
       
    }

    // Same as AI
    void Respawn()
    {
        // Reposition
        var grid = AstarPath.active.data.gridGraph;
        int threshold = 250;

        GraphNode rNode = grid.GetNearest(transform.position).node;


        // Find a walkable position
        for (int i = 0; i < threshold; ++i)
        {
            rNode = grid.nodes[Random.Range(0, grid.nodes.Length)];

            if (rNode.Walkable == true)
            {
                break;
            }
        }
        
        transform.position = (Vector3)rNode.position;

        // Reset
        animator.SetBool("Dead", false);
        aILogic.currentState = AILogic.AI_State.idle;
        parameters.ResetHealth();
    }
}
