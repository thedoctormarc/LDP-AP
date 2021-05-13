using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    //  public float Gravity = 9.8f;
    //  private float velocity = 0;


    // Start is called before the first frame update
    void Start()
    {
        camera = transform.Find("Line Of Sight").GetComponent<Camera>();
        controller = GetComponent<CharacterController>();
        parameters = GetComponent<Parameters>();
        aILogic = GetComponent<AILogic>();
        wParameters = aILogic._weaponSlot().transform.GetChild(0).GetComponent<WeaponParameters>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(camera.transform.position, camera.transform.forward * 100f, Color.white);

        MovementInput();
        AimInput();
        FireInput();
    }


    void MovementInput()
    {
        float x = Input.GetAxis("Horizontal") * parameters._walkSpeed();
        float y = Input.GetAxis("Vertical") * parameters._walkSpeed();
        controller.Move((transform.right * x + transform.forward * y) * Time.deltaTime);

        /*// Gravity
        if (controller.isGrounded)
        {
            velocity = 0;
        }
        else
        {
            velocity -= Gravity * Time.deltaTime;
            controller.Move(new Vector3(0, velocity, 0));
        }*/
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
                RaycastHit hit;
                bool collide = Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, Mathf.Infinity);
        

                if (collide)
                {

                    /*if (hit.transform.parent.gameObject.CompareTag("Player"))
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
                    }*/
                }
            }
        }
       
    }
}
