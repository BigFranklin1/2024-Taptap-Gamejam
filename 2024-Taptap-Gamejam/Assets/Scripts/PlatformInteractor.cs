using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using KinematicCharacterController.Walkthrough.Crouching;
using UnityEngine.Rendering.Universal;

public class PlatformInteractor : MonoBehaviour
{
    public GameObject interactableObj; // The object to interact with
    private GameObject recordedCaster;
    public GameObject playerManager;          // The player object
    public GameObject playerObj;
    //public GameObject ui;
    public GameObject guidanceUI;
    public GameObject shadowExtremeCaseUI;
    public float interactionRange = 2f; // Range within which interaction is possible
    public GameObject smg;

    private bool isInteracting = false; // Track whether the player is currently interacting
    private bool isInteractingWithShadow = false;
    public bool isEnabled;
    public GameObject followPosition;
    public PlatformTrigger standArea;

    // 定义玩家层和阴影层
    public LayerMask playerLayer;
    public LayerMask shadowLayer;
    private int playerLayerInt;
    private int shadowLayerInt;

    private GuidanceUIController guidanceUIController;
    private ShadowExtremeCaseUIHandler shadowExtremeCaseUIHandler;

    private ShadowMeshGenerator shadowMeshGenerator;

    void Start()
    {
        playerLayerInt = LayerMask.NameToLayer("Player");
        shadowLayerInt = LayerMask.NameToLayer("ShadowMesh");

        guidanceUIController = guidanceUI.GetComponent<GuidanceUIController>();
        shadowExtremeCaseUIHandler = shadowExtremeCaseUI.GetComponent<ShadowExtremeCaseUIHandler>();

        shadowMeshGenerator = smg.GetComponent<ShadowMeshGenerator>();

        isInteracting = false;
        isInteractingWithShadow = false;
    }
    void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }
        if (!isEnabled)
        {
            return;
        }

        // Check if the player is near the interactable object
        //float distanceToInteractable = Vector3.Distance(playerObj.transform.position, transform.position);
        if (standArea.triggered)
        {
            //ui.SetActive(true);
            if (guidanceUIController.currentState == GuidanceUIState.Nothing)
            {
                guidanceUIController.SwitchUI(GuidanceUIState.BeforeInteraction);
            }
            
            // If player is within range and presses the 'E' key
            if (Input.GetKeyDown(KeyCode.R))
            {
                // Debug.Log("CameraFollowPoint: "+  playerManager.GetComponent<MyPlayer>().CameraFollowPoint.rotation);
                if (isInteractingWithShadow && isInteracting)
                {
                    //Debug.Log("isInteractingWithShadow: " + isInteractingWithShadow);
                }
                else
                {

                    isInteracting = !isInteracting; // Toggle interaction state

                    InteractableObject interactable = interactableObj.GetComponent<InteractableObject>();
                    if (interactable != null)
                    {
                        interactable.enableInteraction = isInteracting; // Enable or disable object interaction
                    }

                    MyPlayer playerScript = playerManager.GetComponent<MyPlayer>();
                    if (playerScript != null)
                    {
                        playerScript.enableInteraction = !isInteracting; // Disable or enable player's control
                    }

                    if (isInteracting)
                    {
                        guidanceUIController.SwitchUI(GuidanceUIState.ControlObject);
                        playerManager.GetComponent<MyPlayer>().OrbitCamera.ShadowCastingMode(true);
                        Debug.Log("Started interaction with object.");
                        shadowMeshGenerator.HasGeneratedShadow += HasGeneratedShadowHandler;

                    }
                    else
                    {
                        guidanceUIController.SwitchUI(GuidanceUIState.BeforeInteraction);
                        shadowExtremeCaseUIHandler.SwitchUI(ShadowState.Normal);
                        playerManager.GetComponent<MyPlayer>().OrbitCamera.ShadowCastingMode(false);
                        Debug.Log("Ended interaction with object.");
                        shadowMeshGenerator.HasGeneratedShadow -= HasGeneratedShadowHandler;

                    }
                }
            }

            //Debug.Log("isInteractingWithShadow: " + isInteractingWithShadow);

            if (isInteracting)
            {
                if (!isInteractingWithShadow)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        shadowMeshGenerator.CatchShadow();
                    }
                }
                else if (isInteractingWithShadow)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        EnablePlayerShadowCollision(true);

                        isInteractingWithShadow = false;
                        interactableObj.GetComponent<Rigidbody>().isKinematic = false;
                        interactableObj.GetComponent<InteractableObject>().enableInteraction = false;
                        guidanceUIController.SwitchUI(GuidanceUIState.ControlObject);

                        playerManager.GetComponent<MyPlayer>().OrbitCamera.ShadowCastingMode(true);
                        // playerManager.GetComponent<MyPlayer>().CameraFollowPoint = GameObject.Find("CameraFollowPoint").transform;
                        playerManager.GetComponent<MyPlayer>().CameraFollowPoint = followPosition.transform;
                        playerManager.GetComponent<MyPlayer>().CameraFollowPoint.rotation = Quaternion.Euler(0, 205, 0);
                        playerManager.GetComponent<MyPlayer>().OrbitCamera.SetFollowTransform(playerManager.GetComponent<MyPlayer>().CameraFollowPoint);

                        interactableObj = recordedCaster;
                        interactableObj.GetComponent<InteractableObject>().enableInteraction = true;
                    }
                }
            }
        }
        else
        {
            //ui.SetActive(false);
            guidanceUIController.SwitchUI(GuidanceUIState.Nothing);
        }
    }

    private void HasGeneratedShadowHandler(GameObject generatedShadowMesh)
    {
        interactableObj.GetComponent<InteractableObject>().enableInteraction = false;
        recordedCaster = interactableObj;

        isInteractingWithShadow = true;
        interactableObj = generatedShadowMesh;
        interactableObj.GetComponent<InteractableObject>().enableInteraction = true;
        guidanceUIController.SwitchUI(GuidanceUIState.ControlShadow);

        playerManager.GetComponent<MyPlayer>().OrbitCamera.ShadowCastingMode(false);
        playerManager.GetComponent<MyPlayer>().CameraFollowPoint = interactableObj.transform;
        playerManager.GetComponent<MyPlayer>().OrbitCamera.SetFollowTransformInverseDirection(interactableObj.transform);
        EnablePlayerShadowCollision(false);
    }

    private void EnablePlayerShadowCollision(bool enable)
    {
        if (enable)
        {
            // 开启玩家与阴影的碰撞
            Physics.IgnoreLayerCollision(playerLayerInt, shadowLayerInt, false);
            playerObj.GetComponent<MyCharacterController>().Motor.CollidableLayers |= (1 << shadowLayerInt);
        }
        else
        {
            // 关闭玩家与阴影的碰撞
            Physics.IgnoreLayerCollision(playerLayerInt, shadowLayerInt, true);
            playerObj.GetComponent<MyCharacterController>().Motor.CollidableLayers &= ~(1 << shadowLayerInt);
        }
    }

    private void OnDisable()
    {
        //shadowMeshGenerator.HasGeneratedShadow -= HasGeneratedShadowHandler;
    }
}

