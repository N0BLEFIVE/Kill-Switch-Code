using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;


public class GameControllerAnimated : MonoBehaviour
{
    public GameObject player;
    public GameObject initialSpawnPos;
    public GameObject respawnPoint;
    public GameObject cameraHolder;
    public GameObject cameraPosition;
    public GameObject deadBody;
    private GameObject tempBody;
    private GameObject currentBody;
    private GameObject temp;
    private GameObject hips;
    private GameObject spine;
    private GameObject head;
    private GameObject leftUpLeg;
    private GameObject leftLeg;
    private GameObject rightUpLeg;
    private GameObject rightLeg;
    private GameObject spineTemp;
    private GameObject leftArm;
    private GameObject leftForeArm;
    private GameObject leftHand;
    private GameObject rightArm;
    private GameObject rightForeArm;
    private GameObject rightHand;
    private GameObject tempObj;
    public GameObject spawnedPlayer;
    public PlayerControllerAnimated spawnedPlayerScript;
    public GameObject blackOutSquare;
    private Image blackOutSquareImage;
    public GameObject reticleCanvas;
    public TMP_Text pickUpText;
    public GameObject pickUpCanvas;

    public bool startRespawn = false;
    public bool isRespawn = false;
    private bool bodyMoved = false;
    public bool fadeOut = true;
    public bool hitGround;
    public bool respawnAllowed;
    public float ragdollTurnOffDelay;

    public Color objectColor;

    public float j;
    public float fadeSpeed = 2;
    private float fadeAmount;

    public int bodyCount;
    public int deathBySpikesCount;
    public int deathByShreddersCount;
    public int deathByElectricityCount;
    public int deathByBuzzSawCount;
    public int deathByGearBoxCount;
    public int deathByFallingCount;
    public int bodyLimit = 9999;
    public int bodiesUsed;
    public float volume;
    public bool finalRoomSpawn = false;



    void Start()
    {
        Time.timeScale = 1.0f;
        Instantiate(player, initialSpawnPos.transform.position, initialSpawnPos.transform.rotation);
        CheckExistence();


        hitGround = false;
        //objectColor = blackOutSquare.GetComponent<Image>().color;
        objectColor = new Color(0, 0, 0, 0);
        bodyCount = 0;
        blackOutSquareImage = blackOutSquare.GetComponent<Image>();

        if (!finalRoomSpawn)
        {
            Debug.Log(finalRoomSpawn);
            StartCoroutine(FadeBlackOutSqaure(false, 0.2f));
        }
    }

    void CheckExistence()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");

        if (playerObject != null)
        {
            spawnedPlayer = playerObject;
            cameraHolder = GameObject.FindWithTag("CameraHolder");
            cameraPosition = GameObject.FindWithTag("CameraPosition");
            spawnedPlayerScript = spawnedPlayer.GetComponent<PlayerControllerAnimated>();
        }
        else
        {
            Debug.Log("Can't find Player...");
        }
    }

    void Update()
    {
        if (startRespawn && !isRespawn && bodiesUsed < bodyLimit && respawnAllowed) //isRespawning makes sure the player cant respawn until the camera has finished moving
        {
            //add timer in to stop soft lock
            //StartCoroutine(respawnPlayer());
            respawnPlayer();
            blackOutSquareImage.color = new Color(0, 0, 0, 0);
        }

        if (cameraHolder.transform.parent == null && tempBody != null)
        {
            temp = tempBody.transform.Find("parent").gameObject;
            hips = temp.transform.Find("mixamorig:Hips1").gameObject;
            spine = hips.transform.Find("mixamorig:Spine").gameObject;
            spine = spine.transform.Find("mixamorig:Spine1").gameObject;
            spineTemp = spine.transform.Find("mixamorig:Spine2").gameObject;
            head = spineTemp.transform.Find("mixamorig:Neck").gameObject;
            temp = head.transform.Find("mixamorig:Head").gameObject;

            cameraHolder.transform.position = temp.transform.position;
            cameraHolder.transform.parent = temp.transform.parent;
        }
        //This if statement is the Second stage of the respawn process and is only triggered once certain parts of the body e.g. the head have hit the ground
        //Comments within this block will be numbered to show the logic flow at run time
        if (hitGround && isRespawn)
        {
            if (fadeOut) //1.This block fades the screen to black once the body has hit the ground
            {
                fadeAmount = objectColor.a + (fadeSpeed * Time.deltaTime);

                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                blackOutSquareImage.color = objectColor;
                j = blackOutSquareImage.color.a;
                //Debug.Log(j);
                if (j > 1)
                {
                    fadeOut = false;
                }
            }
            //3. This block is only called once the player has been moved, the ragdoll has hit the ground and the camera has been moved back inside the player.
            //This block is responsible for unfading the black screen, reseting the respawn variables and resetting collisions between the player and ragdoll
            if (bodyMoved)
            {
                if (!fadeOut)
                {
                    fadeAmount = objectColor.a - (fadeSpeed * Time.deltaTime);

                    objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                    blackOutSquareImage.color = objectColor;
                    j = blackOutSquareImage.color.a;
                    //Debug.Log(j);
                    if (j < 0)
                    {
                        fadeOut = true;
                        bodyMoved = false;
                        hitGround = false;
                        isRespawn = false;
                        startRespawn = false;
                        SetLayer(currentBody);
                        Physics.IgnoreLayerCollision(6, 3, false);
                    }
                }
            }
            //2.This block executes once the screen has faded to black, then the camera is moved back into the player character and the PlayerController script is enabled
            if (j >= 1) //this checks that the alpha value of the blackout sqaure
            {
                cameraHolder.transform.parent = spawnedPlayer.transform;
                cameraHolder.transform.position = cameraPosition.transform.position;
                cameraHolder.transform.rotation = cameraPosition.transform.rotation;
                tempObj = spawnedPlayer.transform.Find("CameraHolder").gameObject;
                //tempObj = tempObj.transform.Find("CameraHolder").gameObject;
                spawnedPlayerScript.enabled = true;
                tempBody = null;
                bodyMoved = true;
                bodyCount++;
                bodiesUsed++;
            }
        }
        else
        {
            hitGround = false;
        }

        //This is debug inputs for testing the fade to black canvas
        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    StartCoroutine(FadeBlackOutSqaure(true, 0.2f));
        //}
        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    StartCoroutine(FadeBlackOutSqaure(false, 0.2f));
        //}

        //This is a debug respawn that allows you to spawn more than one body at a time
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    deadBody.transform.position = spawnedPlayer.transform.position;
        //    deadBody.transform.rotation = spawnedPlayer.transform.rotation;
        //    spawnedPlayer.transform.position = respawnPoint.transform.position;
        //    spawnedPlayer.transform.rotation = respawnPoint.transform.rotation;

        //    for (int i = 0; i < 10; i++)
        //    {
        //        Instantiate(deadBody);
        //    }
        //}
    }

    //This function is the First stage of respawning process, see inline comments for step by step breakdown
    public void respawnPlayer()
    {
        isRespawn = true;
        deadBody.transform.position = spawnedPlayer.transform.position; //The ragdoll that is about to be spawned is set to the players current position and rotation
        deadBody.transform.rotation = spawnedPlayer.transform.rotation;
        //Debug.Log(spawnedPlayer.transform.Find("CameraHolder").gameObject);
        tempObj = spawnedPlayer.transform.Find("CameraHolder").gameObject; //this finds the game object within the player that holds the main camera
        //Debug.Log(tempObj);
        //tempObj = tempObj.transform.Find("CameraHolder").gameObject; 
        cameraHolder.transform.parent = null; //Removes the player as the cameras parent so they can be moved independntly of each other
        spawnedPlayerScript.DropObject(); //This drops any objects that a player is holding
        spawnedPlayerScript.PlayDeathSound();
        spawnedPlayerScript.enabled = false; //This turns off the PlayerController script, this ensures that no input can be taken from the player during respawn
        Physics.IgnoreLayerCollision(6, 3, true); //This sets it so that collisions between the player and ragdoll are ignored
        spawnedPlayer.transform.position = respawnPoint.transform.position; //These two lines move the player to the repsawn point within the room
        spawnedPlayer.transform.rotation = respawnPoint.transform.rotation;
        tempBody = Instantiate(deadBody); //Creates ragdoll where player was standing
        currentBody = tempBody;
        //This block moves through the hierarchy to find the position for the camera within the ragdoll and sets it as the cameras parent
        temp = tempBody.transform.Find("parent").gameObject; 
        hips = temp.transform.Find("mixamorig:Hips1").gameObject;
        spine = hips.transform.Find("mixamorig:Spine").gameObject;
        spine = spine.transform.Find("mixamorig:Spine1").gameObject;
        spineTemp = spine.transform.Find("mixamorig:Spine2").gameObject;
        head = spineTemp.transform.Find("mixamorig:Neck").gameObject;
        temp = head.transform.Find("mixamorig:Head").gameObject;
        temp = temp.transform.Find("CameraPos").gameObject;
        cameraHolder.transform.position = temp.transform.position;
        cameraHolder.transform.parent = temp.transform.parent;
    }

    //This function sets the deafault layer for all of the rigidbody objects that make up the ragdoll
    private void SetLayer(GameObject currentBody)
    {
        hips = currentBody.transform.Find("parent").gameObject;
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

        hips.layer = 0;
        leftUpLeg.layer = 0;
        leftLeg.layer = 0;
        rightUpLeg.layer = 0;
        rightLeg.layer = 0;
        spine.layer = 0;
        leftArm.layer = 0;
        leftForeArm.layer = 0;
        leftHand.layer = 0;
        rightArm.layer = 0;
        rightForeArm.layer = 0;
        hips.layer = 0;
        rightHand.layer = 0;
        head.layer = 0;
    }

    //The function below is used to fade the screen to and from black, it works by changing the alpha value of a canvas in game
    public IEnumerator FadeBlackOutSqaure(bool fadeToBlack = true, float fadeSpeed = 5f)
    {
        Color objectColorA = blackOutSquareImage.color;
        float fadeAmount;

        if (fadeToBlack)
        {
            while (blackOutSquareImage.color.a < 1)
            {
                fadeAmount = objectColorA.a + (fadeSpeed * Time.deltaTime);
                objectColorA = new Color(objectColorA.r, objectColorA.g, objectColorA.b, fadeAmount);
                blackOutSquareImage.color = objectColorA;
                yield return null;
            }
        }
        else
        {
            while (blackOutSquareImage.color.a > 0)
            {
                fadeAmount = objectColorA.a - (fadeSpeed * Time.deltaTime);
                objectColorA = new Color(objectColorA.r, objectColorA.g, objectColorA.b, fadeAmount);
                blackOutSquareImage.color = objectColorA;
                yield return null;
            }
        }
    }

}
