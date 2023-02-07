using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class PlayerControllerAnimated : MonoBehaviour
{
    public CharacterController controller;
    public PauseV2 menuScript;
    public InputSystemUIInputModule menuInput;
    public PlayerInput playerInput;
    public GameControllerAnimated gameController;

    public Transform holdPos;
    public Transform holdPosObject;
    public Transform playerBody;

    public GameObject player;
    public GameObject menu;
    public GameObject animatorObj;
    public GameObject cameraObj;
    public GameObject heldObj;
    private GameObject hips;
    private GameObject leftUpLeg;
    private GameObject leftLeg;
    private GameObject rightUpLeg;
    private GameObject rightLeg;
    private GameObject spine;
    private GameObject spineTemp;
    private GameObject leftArm;
    private GameObject leftForeArm;
    private GameObject leftHand;
    private GameObject rightArm;
    private GameObject rightForeArm;
    private GameObject rightHand;
    private GameObject head;
    private GameObject tempObj;

    private Rigidbody heldObjRb;

    private bool canThrow = true;
    private bool finishedDropping = true;
    private bool noDoubleJump = true;

    public float mouseSensitivity = 100f;
    public float throwForce = 5f;
    public float throwForceObject = 15f;
    public float pickUpRange = 20f;
    public float speed = 10.0f;
    public float speedTemp = 10.0f;
    public float jumpForce = 10.0f;
    public float gravity = 20.0f;
    public float jumpGracePeriod = 0.2f;
    public float rampUpModifier = 1.1f;
    private float mouseInputX;
    private float mouseInputY;
    private float xRotation = 0f;

    private double startTime;
    private double tempTime;

    public int rampUpOffset = 6;
    private int deathGruntInt;

    public Vector3 movementDir = Vector3.zero;
    private Vector3 movementInput = Vector3.zero;
    private Vector3 airMovementDir = Vector3.zero;

    private RaycastHit pickUpHit;
    private RaycastHit cursorHit;

    public AudioSource StepSource;
    public AudioSource deathSource;
    private AudioClip[] deathSounds = new AudioClip[4];

    private Animator bodyController;


    void Start()
    {
        bodyController = animatorObj.GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        tempObj = GameObject.Find("GameController");
        gameController = tempObj.GetComponent<GameControllerAnimated>();
        menu = GameObject.Find("PauseMenuV2");
        menuScript = menu.GetComponent<PauseV2>();
        Cursor.lockState = CursorLockMode.Locked;
        menu = menu.gameObject.transform.Find("EventSystem").gameObject;
        menuInput = menu.GetComponent<InputSystemUIInputModule>();
        GetComponent<PlayerInput>().uiInputModule = menuInput;
        playerInput = gameObject.GetComponent<PlayerInput>();


        StepSource = this.gameObject.GetComponent<AudioSource>();

        deathSounds[0] = Resources.Load("DeathGrunt1") as AudioClip;
        deathSounds[1] = Resources.Load("DeathGrunt2") as AudioClip;
        deathSounds[2] = Resources.Load("DeathGrunt3") as AudioClip;
        deathSounds[3] = Resources.Load("DeathWilhelm") as AudioClip;

        speedTemp = speed;
        speedTemp -= rampUpOffset;
    }

    void Update()
    {

        //Debug.Log(controller.velocity);
        //Debug.Log(controller.isGrounded);

        PlayerLook();
        PlayerMove();

        //Hoping this will fix weird edge case where if the ragdoll is destoryed while being held, the next time the player respawns they fall through the floor
        if (heldObj == null && finishedDropping)
        {
            ToggleCollisions(false);
        }


        if (Physics.Raycast(cameraObj.transform.position, cameraObj.transform.TransformDirection(Vector3.forward), out cursorHit, pickUpRange))
        {
            //Debug.DrawLine(cameraObj.transform.position, cursorHit.point, Color.white, 5f);

            if ((cursorHit.transform.gameObject.CompareTag("canPickUpObject") || cursorHit.transform.gameObject.CompareTag("canPickUpDeath") || cursorHit.transform.gameObject.CompareTag("canPickUp")) && heldObj == null)
            {
                //Debug.Log(cursorHit.transform.gameObject.name);
                gameController.reticleCanvas.SetActive(false);
                gameController.pickUpCanvas.SetActive(true);

                if (playerInput.currentControlScheme == "Controller")
                {
                    gameController.pickUpText.text = "X";
                }
                else
                {
                    gameController.pickUpText.text = "E";
                }
            }
            else
            {
                gameController.reticleCanvas.SetActive(true);
                gameController.pickUpCanvas.SetActive(false);
            }
        }
        else
        {
            gameController.reticleCanvas.SetActive(true);
            gameController.pickUpCanvas.SetActive(false);
        }



        if (heldObj != null)
        {
            MoveObject();
            //if (Input.GetKeyDown(KeyCode.Mouse0) && canThrow == true)
            //{
            //    ThrowObject();
            //}
        }
    }

    //This respawns the player when the correct input is given
    private void OnRespawn()
    {
        gameController.startRespawn = true;
    }

    //This function allows the player to pause the game
    private void OnPause()
    {
        if (!menuScript.startPause)
        {
            menuScript.startPause = true;

        }
        else
        {
            menuScript.startPause = false;
        }
    }

    //This function allows the player to use the back button in the main menu
    //This should be a part of the menu code but is here due to limitations in unitys input system
    private void OnBack()
    {
        if (menuScript.paused)
        {
            menuScript.Back();
        }
    }

    //This function allows players to pick up bodies and objects in the game world
    /*When the player presses the pick up input a raycast is sent from the camera which checks the tags of any items
     *it collides with and calls the relevant pick up function if the object can be picked up*/
    private void OnPickUp()
    {
            if (heldObj == null)
            {
                if (Physics.Raycast(cameraObj.transform.position, cameraObj.transform.TransformDirection(Vector3.forward), out pickUpHit, pickUpRange))
                {
                    //Debug.DrawLine(cameraObj.transform.position, pickUpHit.point, Color.white, 5f);
                    //Debug.Log(pickUpHit.transform.gameObject.tag);
                    //Debug.Log(pickUpHit.transform.gameObject.name);
                    if (pickUpHit.transform.gameObject.CompareTag("canPickUpDeath") || pickUpHit.transform.gameObject.CompareTag("canPickUp"))
                    {
                        PickUpBody(pickUpHit.transform.gameObject);
                    }
                    if (pickUpHit.transform.gameObject.CompareTag("canPickUpObject"))
                    {
                        PickUpObject(pickUpHit.transform.gameObject);
                    }
                }
            }
            else
            {
                DropObject();
            }
    }

    /*This functions allows the ragdoll bodies to be picked up. The function finds all parts of the ragdoll that have rigidbodies attached 
     * in order to enable selective collisions. The hips are then set as the held object, they are transformed to the hold position, the ragdoll is re-enabled
     * and collisions between the ragdoll and the player character and turned off
     */
    private void PickUpBody(GameObject pickUpObj)
    {
        if (pickUpObj.GetComponent<Rigidbody>())
        {
            heldObj = pickUpObj;
            heldObj = heldObj.transform.root.gameObject;
            //finding all of the colliders so we can ignore collsions from them
            hips = heldObj.transform.Find("parent").gameObject;
            hips = hips.transform.Find("mixamorig:Hips1").gameObject;
            leftUpLeg = hips.transform.Find("mixamorig:LeftUpLeg").gameObject;
            leftLeg = leftUpLeg.transform.Find("mixamorig:LeftLeg").gameObject;
            rightUpLeg = hips.transform.Find("mixamorig:RightUpLeg").gameObject;
            rightLeg = rightUpLeg.transform.Find("mixamorig:RightLeg").gameObject;
            spine = hips.transform.Find("mixamorig:Spine").gameObject;
            spine = spine.transform.Find("mixamorig:Spine1").gameObject;

            spineTemp = spine.transform.Find("mixamorig:Spine2").gameObject;

            leftArm = spineTemp.transform.Find("mixamorig:LeftShoulder").gameObject;
            leftArm = leftArm.transform.Find("mixamorig:LeftArm").gameObject;

            leftForeArm = leftArm.transform.Find("mixamorig:LeftForeArm").gameObject;
            leftHand = leftForeArm.transform.Find("mixamorig:LeftHand").gameObject;

            rightArm = spineTemp.transform.Find("mixamorig:RightShoulder").gameObject;
            rightArm = rightArm.transform.Find("mixamorig:RightArm").gameObject;

            rightForeArm = rightArm.transform.Find("mixamorig:RightForeArm").gameObject;
            rightHand = rightForeArm.transform.Find("mixamorig:RightHand").gameObject;
            head = spineTemp.transform.Find("mixamorig:Neck").gameObject;
            head = head.transform.Find("mixamorig:Head").gameObject;


            heldObj = hips;
            heldObjRb = heldObj.GetComponent<Rigidbody>();
            //heldObjRb.transform.position = holdPos.transform.position;
            //Debug.Log("----------------------------------------");
            //Debug.Log(heldObj);
            //Debug.Log(heldObjRb);
            //Debug.Log("----------------------------------------");
            heldObjRb.transform.position = holdPos.transform.position;
            heldObjRb.transform.rotation = holdPos.transform.rotation;

            //heldObj.gameObject.transform.root.gameObject.layer = 6;

            heldObj.GetComponent<RagdollScriptAnimated>().TurnOnRagdoll();
            ToggleLayer(6);
            ToggleCollisions(true);
        }
    }

    //This function handles the players movement
    private void PlayerMove()
    {
        //Debug.Log(Input.GetAxisRaw("Horizontal"));
        //Debug.Log(Input.GetAxisRaw("Vertical"));

        //The if statements below check for movement input and either ramps up their speed or sets it back to the baseline when not moving 
        if ((movementInput.z != 0 || movementInput.x != 0) && speedTemp <= speed)
        {
            speedTemp *= rampUpModifier;
        }
        else if (movementInput.z == 0 && movementInput.x == 0)
        {
            speedTemp = speed;
            speedTemp -= rampUpOffset;
        }

        //This if statement normalises the movement input
        if (movementInput.magnitude > 1.0f)
        {
            movementInput = movementInput.normalized;
        }

        //This if is used when the player is moving in the air
        if (!controller.isGrounded)
        {
            airMovementDir = new Vector3(movementInput.x, movementInput.y, movementInput.z);

            startTime = Time.timeAsDouble;

            airMovementDir.x *= speedTemp;
            airMovementDir.z *= speedTemp;

            movementDir.x = airMovementDir.x;
            movementDir.z = airMovementDir.z;

        }

        //This if is used when the player is moving on the ground
        if (controller.isGrounded)
        {
            movementDir = new Vector3(movementInput.x, movementInput.y, movementInput.z);

            movementDir.x *= speedTemp;
            movementDir.z *= speedTemp;
            startTime = Time.timeAsDouble;
            tempTime = startTime + jumpGracePeriod;
        }

        movementDir = transform.TransformDirection(movementDir);
        controller.Move(movementDir * Time.deltaTime);

        //This if statement plays a stepping sound when the player is moving and is touching the ground
        if (controller.velocity.magnitude > 2f && StepSource.isPlaying == false && controller.isGrounded)
        {
            StepSource.Play();
        }

        //The three if statements below change the characters animations based on its movement and if it is touching the ground
        if (controller.velocity.magnitude > 1f && controller.isGrounded)
        {
            bodyController.SetBool("Walking", true);
        }
        else if (controller.velocity.magnitude < 1f && !controller.isGrounded)
        {
            bodyController.SetBool("Walking", false);

        }
        if (controller.isGrounded)
        {
            bodyController.SetBool("Jumping", false);
        }


        movementDir.y -= gravity * Time.deltaTime;
        movementDir.x -= gravity * Time.deltaTime;
        movementDir.z -= gravity * Time.deltaTime;
        //Debug.Log(movementDir);

    }

    //This function is called when the player uses movement input
    private void OnMove(InputValue value)
    {
        movementInput = new Vector3(value.Get<Vector2>().x, movementInput.y, value.Get<Vector2>().y);
        //Debug.Log(value.Get<Vector2>());
    }

    //This function handles the players look in the game world
    private void PlayerLook()
    {
        xRotation -= mouseInputY;
        xRotation = Mathf.Clamp(xRotation, -80f, 90f); //This limits the players look rotation

        cameraObj.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseInputX);
    }

    //This function is called when the player uses look input
    private void OnLook(InputValue value)
    {
        mouseInputX = value.Get<Vector2>().x * mouseSensitivity * Time.deltaTime;
        mouseInputY = value.Get<Vector2>().y * mouseSensitivity * Time.deltaTime;
    }

    /*This function is called when the player uses jump input. It uses timers that are set in the Movement functions to allow the player
     * to still jump for a short period of time after they begin falling.*/
    private void OnJump()
    {
        if (!controller.isGrounded)
        {

            if (startTime < tempTime)
            {
                bodyController.SetBool("Jumping", true);
                movementDir.y = jumpForce;
                tempTime = startTime;
            }
        }
        else 
        {
            bodyController.SetBool("Jumping", true);
            movementDir.y = jumpForce;
        }
        movementDir = transform.TransformDirection(movementDir);
        controller.Move(movementDir * Time.deltaTime);
    }

    /*This function allows the player to pick up game objects. It finds and transforms the held object, while also ensuring that the object 
    and player do not collide*/
    private void PickUpObject(GameObject pickUpObj)
    {
        if (pickUpObj.GetComponent<Rigidbody>())
        {
            heldObj = pickUpObj;
            //Debug.Log(heldObj);
            heldObjRb = heldObj.GetComponent<Rigidbody>();
            heldObjRb.isKinematic = false;
            ToggleLayer(6);
            ToggleCollisions(true);
            heldObjRb.transform.position = holdPosObject.transform.position;

        }

    }
    
    //This function allows the player to drop held objects. It handles both Ragdolls and game objects and ensures that collisions are reenabled.
    public void DropObject()
    {
        if (heldObj != null)
        {
            if (heldObj.GetComponent<RagdollScriptAnimated>() == true) 
            {
                //These are Coroutines to stop the body coliding with the player when dropped
                StartCoroutine(ToggleCollisionsDrop(false));
                StartCoroutine(ToggleLayerDrop(0));
                
                finishedDropping = false;
                heldObj.GetComponent<RagdollScriptAnimated>().TurnOnRagdoll();
                heldObj = null;
            }
            else
            {
                ToggleLayer(0);
                ToggleCollisions(false);
                heldObj = null;
            }
        }
    }
    
    /*This function moves the object the player is holding along with them, it also ensures that the velocity of the held object is clamped 
    due to an issue of held objects gaining infinite velocity*/
    private void MoveObject()
    {
        if (heldObj.GetComponent<RagdollScriptAnimated>() == true) 
        {
            ToggleLayer(6);
            ToggleCollisions(true);
            heldObj.GetComponent<RagdollScriptAnimated>().TurnOnRagdoll();
            heldObjRb.transform.position = holdPos.transform.position;
            heldObjRb.transform.rotation = holdPos.transform.rotation;
            heldObj.GetComponent<RagdollScriptAnimated>().ClampVelocity();
        }
        else
        {
            heldObjRb.transform.position = holdPosObject.transform.position;

            if (heldObjRb.velocity.magnitude > 5)
            {
                heldObjRb.GetComponent<Rigidbody>().velocity = new Vector3(0, -5, 0);
            }
        }
    }

    //This functions allows the player to throw a held object. It handles both Ragdolls and game objects, applying a force to them and reenabling collisions
    private void ThrowObject()
    {
        
        if (heldObj.GetComponent<RagdollScriptAnimated>() == true) 
        {
            heldObj.GetComponent<RagdollScriptAnimated>().TurnOnRagdoll();
            StartCoroutine(ToggleCollisionsDrop(false));
            StartCoroutine(ToggleLayerDrop(0));
            finishedDropping = false;

            heldObjRb.transform.rotation = cameraObj.transform.rotation;
            //The block of statements below ensures that the throw force is applied to each part of the ragdoll
            hips.GetComponent<Rigidbody>().velocity = (cameraObj.transform.forward * throwForce);
            leftUpLeg.GetComponent<Rigidbody>().velocity = (cameraObj.transform.forward * throwForce);
            leftLeg.GetComponent<Rigidbody>().velocity = (cameraObj.transform.forward * throwForce);
            rightUpLeg.GetComponent<Rigidbody>().velocity = (cameraObj.transform.forward * throwForce);
            rightLeg.GetComponent<Rigidbody>().velocity = (cameraObj.transform.forward * throwForce);
            spine.GetComponent<Rigidbody>().velocity = (cameraObj.transform.forward * throwForce);
            leftArm.GetComponent<Rigidbody>().velocity = (cameraObj.transform.forward * throwForce);
            leftForeArm.GetComponent<Rigidbody>().velocity = (cameraObj.transform.forward * throwForce);
            leftHand.GetComponent<Rigidbody>().velocity = (cameraObj.transform.forward * throwForce);
            rightArm.GetComponent<Rigidbody>().velocity = (cameraObj.transform.forward * throwForce);
            rightForeArm.GetComponent<Rigidbody>().velocity = (cameraObj.transform.forward * throwForce);
            hips.GetComponent<Rigidbody>().velocity = (cameraObj.transform.forward * throwForce);
            rightHand.GetComponent<Rigidbody>().velocity = (cameraObj.transform.forward * throwForce);
            head.GetComponent<Rigidbody>().velocity = (cameraObj.transform.forward * throwForce);

            heldObj = null;
        }
        else if (heldObj.CompareTag("canPickUpObject"))
        {
            ToggleLayer(0);
            ToggleCollisions(false);
            heldObj.GetComponent<Rigidbody>().velocity = (cameraObj.transform.forward * throwForceObject);
            heldObj = null;
        }
    }

    //This function is called when the player uses the throw input
    private void OnThrow()
    {
        if (heldObj != null)
        {
            if (canThrow == true)
            {
                ThrowObject();
            }
        }
    }

    //The two functions below change the collision layer on the ragdoll and and disable collisions between layers.
    public void ToggleCollisions(bool toggle)
    {
        //this ignores collisions between the body and the envrionment but not the gearbox
        Physics.IgnoreLayerCollision(6, 0, toggle);
        Physics.IgnoreLayerCollision(6, 3, toggle);
    }

    private void ToggleLayer(int toggle)
    {
        if (heldObj.GetComponent<RagdollScriptAnimated>() == true) 
        {
            hips.layer = toggle;
            leftUpLeg.layer = toggle;
            leftLeg.layer = toggle;
            rightUpLeg.layer = toggle;
            rightLeg.layer = toggle;
            spine.layer = toggle;
            leftArm.layer = toggle;
            leftForeArm.layer = toggle;
            leftHand.layer = toggle;
            rightArm.layer = toggle;
            rightForeArm.layer = toggle;
            hips.layer = toggle;
            rightHand.layer = toggle;
            head.layer = toggle;
        }
        else
        {
            heldObj.layer = toggle; 
        }

    }

    //The two functions below change the collision layer on the ragdoll and and re-enable collisions between layers.
    //These functions are set as IEnumerators so they can be called on a delay, this is to stop the body from colliding
    //with the player when being dropped.
    private IEnumerator ToggleLayerDrop(int toggle)
    {
        if (heldObj.GetComponent<RagdollScriptAnimated>() == true) //make this a better check
        {
            yield return new WaitForSeconds(0.5f);
            hips.layer = toggle;
            leftUpLeg.layer = toggle;
            leftLeg.layer = toggle;
            rightUpLeg.layer = toggle;
            rightLeg.layer = toggle;
            spine.layer = toggle;
            leftArm.layer = toggle;
            leftForeArm.layer = toggle;
            leftHand.layer = toggle;
            rightArm.layer = toggle;
            rightForeArm.layer = toggle;
            hips.layer = toggle;
            rightHand.layer = toggle;
            head.layer = toggle;
        }
        else
        {
            heldObj.layer = toggle;
        }

    }

    public IEnumerator ToggleCollisionsDrop(bool toggle)
    {
        Physics.IgnoreLayerCollision(6, 0, toggle);
        yield return new WaitForSeconds(0.5f);
        Physics.IgnoreLayerCollision(6, 3, toggle);
        finishedDropping = true;
    }

    //The function below plays a sound each time the player dies
    public void PlayDeathSound()
    {
        deathGruntInt = Random.Range(0, 4); //this randomly picks what death grunt to play
        if (deathGruntInt == 3) //i dont want the wilhelm scream to play as often as the others and this keeps it rare
        {
            deathGruntInt = Random.Range(0, 4);

            if (deathGruntInt == 3)
            {
                deathGruntInt = Random.Range(0, 4);

                if (deathGruntInt == 3)
                {
                    deathGruntInt = Random.Range(0, 4);
                } 
            }
        }
        StepSource.PlayOneShot(deathSounds[deathGruntInt]);
    }

    //This enables the player to use the Debug menu
    //Comment this out for release builds
    private void OnDebug()
    {
        menuScript.DebugMenu();
    }
}
