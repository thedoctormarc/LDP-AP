using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEngine.UI;

public class HumanController : MonoBehaviour
{
    [SerializeField]
    float aimSpeed = 3f;
    [SerializeField]
    Image healthBar;
    [SerializeField]
    Image crosshair;
    [SerializeField]
    Image crossHairInnaccurate;
    [SerializeField]
    Text magazineText;

    private int bullets;
    private float xRot = 0.0f;
    private float yRot = 0.0f;
    private Camera camera;
    private CharacterController controller;
    private WeaponParameters wParameters;
    private Parameters parameters;
    private float currentFireTime = 0f;
    private AILogic aILogic;
    private AudioSource weaponAudio;
    private float currentVel = 0f;
    private float currentRespawnTime = 0f;
    private Animator animator;
    private AudioSource audioSource;
    private bool fireReady = true;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        camera = transform.Find("Line Of Sight").GetComponent<Camera>();
        controller = GetComponent<CharacterController>();
        parameters = GetComponent<Parameters>();
        aILogic = GetComponent<AILogic>();
        GameObject weapon = GetActiveWeapon();
        weaponAudio = weapon.GetComponent<AudioSource>();
        wParameters = weapon.GetComponent<WeaponParameters>();
        bullets = wParameters._capacity();
        magazineText.text = bullets.ToString() + "/" + bullets.ToString();

    }

    GameObject GetActiveWeapon()
    {
        for (int i = 0; i < aILogic._weaponSlot().transform.childCount; ++i)
        {
            GameObject weapon = aILogic._weaponSlot().transform.GetChild(i).gameObject;
            if (weapon.activeSelf)
            {
                return weapon;
            }
        }
        return null;
    }

    void Update()
    {
        Debug.DrawRay(camera.transform.position, camera.transform.forward * 100f, Color.white);

        if (Input.GetKeyDown(KeyCode.F1))
        {
            ChangeTeam();
        }

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
        bool running = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftShift);
        float x = Input.GetAxis("Horizontal") * ((running) ? parameters._runSpeed() : parameters._walkSpeed());
        float y = Input.GetAxis("Vertical") * ((running) ? parameters._runSpeed() : parameters._walkSpeed());
        controller.Move((transform.right * x + transform.forward * y) * Time.deltaTime);

        crosshair.enabled = !running;
        crossHairInnaccurate.enabled = running;

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
        // Cooldown
        if (fireReady == false)
        {
            if ((currentFireTime += Time.deltaTime) >= wParameters._fireTime())
            {
                if (bullets == 0)
                {
                    bullets = wParameters._capacity();
                    magazineText.text = bullets.ToString() + "/" + wParameters._capacity().ToString();
                }
                currentFireTime = 0f;
                fireReady = true;
            }
        }

        // Fire
        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse0))
        {
            if (fireReady)
            {
                weaponAudio.Play();
                RaycastHit hit;

                Vector3 offset = new Vector3();

                // Running innaccuracy
                if (crossHairInnaccurate.enabled)
                {
                    float signedAimSpread = parameters._runningSpread() / 2f;
                    float xOffset = Random.Range(-signedAimSpread, signedAimSpread);
                    float yOffset = Random.Range(-signedAimSpread, signedAimSpread);
                    float zOffset = Random.Range(-signedAimSpread, signedAimSpread);

                    offset = new Vector3(xOffset, yOffset, zOffset);

                }

                bool collide = Physics.Raycast(camera.transform.position + offset, camera.transform.forward, out hit, Mathf.Infinity);


                if (collide)
                {

                    if (hit.transform.parent.gameObject.CompareTag("Player"))
                    {

                        Parameters aIParameters = hit.transform.parent.gameObject.GetComponent<Parameters>();
                        if (aIParameters._team() == parameters._team())
                        {
                            return;
                        }

                        float damage = wParameters.GetDamageAtDistance((hit.point - transform.position).magnitude);
                        PlayerManager.instance.DamageAI(damage, hit.transform.parent.gameObject, gameObject); 
                    }
                }

                if ((--bullets) == 0)
                {
                    audioSource.Play();
                    currentFireTime = -wParameters._reloadTime() + wParameters._fireTime();
                }

                magazineText.text = bullets.ToString() + "/" + wParameters._capacity().ToString();
               
                fireReady = false;
            }
        }
       
    }

    // Same as AI
    void Respawn()
    {
        // Reposition
        var grid = AstarPath.active.data.gridGraph;
        int threshold = 500;

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
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        healthBar.transform.localScale = new Vector3(parameters._currentHealth() / parameters._maxHealth(), 1f, 1f);
    }

    void ChangeTeam()
    {
        aILogic._weaponSlot().transform.GetChild(parameters.team).gameObject.SetActive(false);
        if ((++parameters.team) > 2)
        {
            parameters.team = 0;
        }

        GameObject weapon = aILogic._weaponSlot().transform.GetChild(parameters.team).gameObject;
        weapon.SetActive(true);
        weaponAudio = weapon.GetComponent<AudioSource>();
        wParameters = weapon.GetComponent<WeaponParameters>();
        currentFireTime = 0f;
        bullets = wParameters._capacity();
        magazineText.text = bullets.ToString() + "/" + wParameters._capacity().ToString();

    }
}
