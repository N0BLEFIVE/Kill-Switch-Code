using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class RagdollScriptAnimated : MonoBehaviour
{
    public bool isElectrified = false;
    public bool turningOff;

    private GameObject ragdoll;
    private GameObject currObj;
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
    private GameObject temp;
    private GameObject root;
    private GameObject player;
    private GameObject characterMesh;

    private Rigidbody hipsRB;
    private Rigidbody leftUpLegRB;
    private Rigidbody leftLegRB;
    private Rigidbody rightUpLegRB;
    private Rigidbody rightLegRB;
    private Rigidbody spineRB;
    private Rigidbody spineTempRB;
    private Rigidbody leftArmRB;
    private Rigidbody leftForeArmRB;
    private Rigidbody leftHandRB;
    private Rigidbody rightArmRB;
    private Rigidbody rightForeArmRB;
    private Rigidbody rightHandRB;
    private Rigidbody headRB;
    private Rigidbody thisRigidBody;

    private RagdollScriptAnimated headScript;

    private GameControllerAnimated gameController;

    private Renderer characterRenderer;

    private MeshCollider respawnTubeCol;

    public ParticleSystem smoke;

    private float turnOffDelay;
    private float electricityEffectModifier;
    private float negative = -1f;

    private double runningTime;
    private double destroyTime;

    private int frames = 0;



    //This start statement finds a number of important game objects and finds all the rigidbodies of the ragdoll
    void Start()
    {
        temp = GameObject.Find("GameController");
        gameController = temp.GetComponent<GameControllerAnimated>();
        turnOffDelay = gameController.GetComponent<GameControllerAnimated>().ragdollTurnOffDelay;
        player = gameController.GetComponent<GameControllerAnimated>().spawnedPlayer;
        ragdoll = gameObject;
        currObj = ragdoll;
        currObj = currObj.transform.root.gameObject;
        root = currObj;
        currObj = currObj.transform.Find("parent").gameObject;
        hips = currObj.transform.Find("mixamorig:Hips1").gameObject;
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

        thisRigidBody = gameObject.GetComponent<Rigidbody>();
        hipsRB = hips.GetComponent<Rigidbody>();
        leftUpLegRB = leftUpLeg.GetComponent<Rigidbody>();
        leftLegRB = leftLeg.GetComponent<Rigidbody>();
        rightUpLegRB = rightUpLeg.GetComponent<Rigidbody>();
        rightLegRB = rightLeg.GetComponent<Rigidbody>();
        spineRB = spine.GetComponent<Rigidbody>();
        leftArmRB = leftArm.GetComponent<Rigidbody>();
        leftForeArmRB = leftForeArm.GetComponent<Rigidbody>();
        leftHandRB = leftHand.GetComponent<Rigidbody>();
        rightArmRB = rightArm.GetComponent<Rigidbody>();
        rightForeArmRB = rightForeArm.GetComponent<Rigidbody>();
        rightHandRB = rightHand.GetComponent<Rigidbody>();
        headRB = head.GetComponent<Rigidbody>();
        headScript = head.GetComponent<RagdollScriptAnimated>();
    }

    void Update()
    {
        runningTime = Time.timeAsDouble;
        //Debug.Log(destroyTime);
        //This destorys the ragdoll if it has not been interacted with by the player for the set time
        if (runningTime > destroyTime && destroyTime != 0)
        {
            Destroy(gameObject.transform.root.gameObject);
        }

        //This resets the destroy timer for the ragdoll when it is picked up
        if (!thisRigidBody.isKenimatic)
        {
            runningTime = 0;
            destroyTime = 0;
        }

        //This block checks whether the ragdoll is electrified and changes the renderer to make it looks electrified
        if (head != null && characterRenderer == null)
        {
            if (headScript.isElectrified)
            {
                characterMesh = head.transform.root.gameObject;
                characterMesh = characterMesh.transform.Find("parent").gameObject;
                characterMesh = characterMesh.transform.Find("KS_CHaracter_Rig_GRP").gameObject;
                characterMesh = characterMesh.transform.Find("Character_Mesh").gameObject;
                characterMesh = characterMesh.transform.Find("CHarcter_Skin_Geo").gameObject;

                characterRenderer = characterMesh.GetComponent<Renderer>();

                characterRenderer.material.SetFloat("_EmissionForHoles", 0f);
            }
        }

        //This block changes how the electricity on a ragdoll looks over time
        if (characterRenderer != null)
        {
            electricityEffectModifier = Mathf.PingPong(Time.time, 0.8f);
            electricityEffectModifier *= negative;
            characterRenderer.material.SetFloat("_EmissionForHoles", electricityEffectModifier);
        }

        //The next two blocks are checks to see when the ragdoll is no longer being used by the player so the destroy timer can start
        if (thisRigidBody.velocity.magnitude == 0 && !gameController.isRespawn && turningOff == false)
        {
            TurnOffRagdoll();
            destroyTime = Time.timeAsDouble + 600;
        }

        if (frames > 240)
        {
            TurnOffRagdoll();
            destroyTime = Time.timeAsDouble + 600;
            turningOff = true;
            frames = 0;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        //The if statement below checks to see if part of the ragdoll has collided with the lightning poles and electrifies the ragdoll if so
        if (other.gameObject.CompareTag("LightningPoles"))
        {
            isElectrified = true;
            hips.GetComponent<RagdollScriptAnimated>().isElectrified = true;
            leftUpLeg.GetComponent<RagdollScriptAnimated>().isElectrified = true;
            leftLeg.GetComponent<RagdollScriptAnimated>().isElectrified = true;
            rightUpLeg.GetComponent<RagdollScriptAnimated>().isElectrified = true;
            rightLeg.GetComponent<RagdollScriptAnimated>().isElectrified = true;
            spine.GetComponent<RagdollScriptAnimated>().isElectrified = true;
            leftArm.GetComponent<RagdollScriptAnimated>().isElectrified = true;
            leftForeArm.GetComponent<RagdollScriptAnimated>().isElectrified = true;
            leftHand.GetComponent<RagdollScriptAnimated>().isElectrified = true;
            rightArm.GetComponent<RagdollScriptAnimated>().isElectrified = true;
            rightForeArm.GetComponent<RagdollScriptAnimated>().isElectrified = true;
            rightHand.GetComponent<RagdollScriptAnimated>().isElectrified = true;
            headScript.isElectrified = true;
        }

        //This block stops the ragdoll from moving when it enters a gear box
        if (other.gameObject.CompareTag("GearBox"))
        {
            if (!gameController.isRespawn)
            {
                if (other.gameObject.GetComponent<Rigidbody>() != null)
                {
                    other.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                }
            }
        }
    }

    //This block re-enables the ragdolls movement when it is removed from objects such as the gear box
    private void OnTriggerExit(Collider other)
    {
        if (!gameController.isRespawn)
        {
            if (other.gameObject.GetComponent<Rigidbody>() != null)
            {
                other.gameObject.GetComponent<Rigidbody>().isKinematic = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(gameObject.transform.root.gameObject.name + gameObject.name);
        //Debug.Log(collision.gameObject.transform.root.gameObject.name + collision.gameObject.name);

        //The below if statement ignores collisions between the ragdoll and the respawn tube, this stops the player getting stuck
        //if they respawn in the respawn tube.
        if (collision.gameObject.CompareTag("RespawnTube"))
        {
            respawnTubeCol = collision.gameObject.GetComponent<MeshCollider>();
            Physics.IgnoreCollision(respawnTubeCol, hips.GetComponent<Collider>(), true);
            Physics.IgnoreCollision(respawnTubeCol, leftUpLeg.GetComponent<Collider>(), true);
            Physics.IgnoreCollision(respawnTubeCol, leftLeg.GetComponent<Collider>(), true);
            Physics.IgnoreCollision(respawnTubeCol, rightUpLeg.GetComponent<Collider>(), true);
            Physics.IgnoreCollision(respawnTubeCol, rightLeg.GetComponent<Collider>(), true);
            Physics.IgnoreCollision(respawnTubeCol, spine.GetComponent<Collider>(), true);
            Physics.IgnoreCollision(respawnTubeCol, leftArm.GetComponent<Collider>(), true);
            Physics.IgnoreCollision(respawnTubeCol, leftForeArm.GetComponent<Collider>(), true);
            Physics.IgnoreCollision(respawnTubeCol, leftHand.GetComponent<Collider>(), true);
            Physics.IgnoreCollision(respawnTubeCol, rightArm.GetComponent<Collider>(), true);
            Physics.IgnoreCollision(respawnTubeCol, rightForeArm.GetComponent<Collider>(), true);
            Physics.IgnoreCollision(respawnTubeCol, rightHand.GetComponent<Collider>(), true);
            Physics.IgnoreCollision(respawnTubeCol, head.GetComponent<Collider>(), true);
        }

        //The nested if statements below are used to check when the ragdoll has hit he ground while respawning so the camera can be moved back to the player.
        if (collision.gameObject.transform.root.gameObject != gameObject.transform.root.gameObject)
        {
            //Debug.Log(gameObject.transform.root.gameObject.name + gameObject.name);
            //Debug.Log(collision.gameObject.transform.root.gameObject.name + collision.gameObject.name);
            if (gameObject.CompareTag("canPickUpDeath"))
            {
                if ((collision.gameObject.CompareTag("DeathSurface") || collision.gameObject.CompareTag("RespawnTube") || collision.gameObject.CompareTag("canPickUp") || collision.gameObject.CompareTag("Spikes") || collision.gameObject.CompareTag("GearBox")) && gameController.hitGround == false)
                {
                    gameController.hitGround = true;
                }
            }
        }


        if (collision.gameObject.CompareTag("DeathSurface") || collision.gameObject.CompareTag("RespawnTube") || collision.gameObject.CompareTag("Spikes") || collision.gameObject.CompareTag("GearBox"))
        {
            Debug.Log(gameObject.transform.root.root.gameObject);
            root.transform.SetParent(null, true);
        }
    }

    //the block below is used to count how long the ragdoll is touching a surface, this is used to turn off the ragdolls movement when not in use
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.transform.root.gameObject != gameObject.transform.root.gameObject)
        {
            if (collision.gameObject.CompareTag("DeathSurface") || collision.gameObject.CompareTag("canPickUpDeath") || collision.gameObject.CompareTag("canPickUp"))
            {
                if (!turningOff && !gameController.isRespawn)
                {
                    if (thisRigidBody.velocity.magnitude < 0.1)
                    {
                        frames++;
                    }
                }
            }
        }
    }

    //This block resets the timer that is incremented in the block above
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.transform.root.gameObject != gameObject.transform.root.gameObject)
        {

            if (gameObject.CompareTag("canPickUpDeath"))
            {
                if (collision.gameObject.CompareTag("DeathSurface") || gameObject.CompareTag("canPickUpDeath") || gameObject.CompareTag("canPickUp"))
                {
                    frames = 0;
                }
            }
        }
    }


    //The two code blocks below are used to turn on & off the ragdolls movement
    public void TurnOffRagdoll()
    {
        turningOff = true;
        hipsRB.isKinematic = true;
        leftUpLegRB.isKinematic = true;
        leftLegRB.isKinematic = true;
        rightUpLegRB.isKinematic = true;
        rightLegRB.isKinematic = true;
        spineRB.isKinematic = true;
        leftArmRB.isKinematic = true;
        leftForeArmRB.isKinematic = true;
        leftHandRB.isKinematic = true;
        rightArmRB.isKinematic = true;
        rightForeArmRB.isKinematic = true;
        rightHandRB.isKinematic = true;
        headRB.isKinematic = true;
    }

    public void TurnOnRagdoll()
    {
        turningOff = false;
        hipsRB.isKinematic = false;
        leftUpLegRB.isKinematic = false;
        leftLegRB.isKinematic = false;
        rightUpLegRB.isKinematic = false;
        rightLegRB.isKinematic = false;
        spineRB.isKinematic = false;
        leftArmRB.isKinematic = false;
        leftForeArmRB.isKinematic = false;
        leftHandRB.isKinematic = false;
        rightArmRB.isKinematic = false;
        rightForeArmRB.isKinematic = false;
        rightHandRB.isKinematic = false;
        headRB.isKinematic = false;
    }

    //This function clamps the velocity of the ragdolls body parts
    public void ClampVelocity()
    {
        int clamp = -5;
        hipsRB.velocity = new Vector3(0, clamp, 0);
        leftUpLegRB.velocity = new Vector3(0, clamp, 0);
        leftLegRB.velocity = new Vector3(0, clamp, 0);
        rightUpLegRB.velocity = new Vector3(0, clamp, 0);
        rightLegRB.velocity = new Vector3(0, clamp, 0);
        spineRB.velocity = new Vector3(0, clamp, 0);
        leftArmRB.velocity = new Vector3(0, clamp, 0);
        leftForeArmRB.velocity = new Vector3(0, clamp, 0);
        leftHandRB.velocity = new Vector3(0, clamp, 0);
        rightArmRB.velocity = new Vector3(0, clamp, 0);
        rightForeArmRB.velocity = new Vector3(0, clamp, 0);
        rightHandRB.velocity = new Vector3(0, clamp, 0);
        headRB.velocity = new Vector3(0, clamp, 0);
    }

    //This function destroys the ragdoll and creates smoke at its last position
    private void OnDestroy()
    {
        smoke.transform.position = this.gameObject.transform.position;
        Instantiate(smoke);
    }
}
