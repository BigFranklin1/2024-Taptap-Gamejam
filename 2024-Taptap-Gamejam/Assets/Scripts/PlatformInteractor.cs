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
    public GameObject playerManager;          // The player object
    public GameObject playerObj;
    public GameObject ui;
    public float interactionRange = 2f; // Range within which interaction is possible
    public GameObject smg;
    private bool isInteracting = false; // Track whether the player is currently interacting
    private bool isInteractingWithShadow = false;
    public bool isEnabled;
    public AudioSource shadowSound;      // 按钮触发音效
    public GameObject followPosition;
    public PlatformActiveDetection standArea;
    
    

    void Update()
    {
        if (!isEnabled)
        {
            return;
        }

        // Check if the player is near the interactable object
        //float distanceToInteractable = Vector3.Distance(playerObj.transform.position, transform.position);
        if (standArea.triggered)
        {
            ui.SetActive(true);
            // If player is within range and presses the 'E' key
            if (Input.GetKeyDown(KeyCode.R))
            {
                // Debug.Log("CameraFollowPoint: "+  playerManager.GetComponent<MyPlayer>().CameraFollowPoint.rotation);

                isInteracting = !isInteracting; // Toggle interaction state
                if(isInteractingWithShadow)
                {
                    isInteractingWithShadow = false;
                }
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
                    Debug.Log("Started interaction with object.");
                }
                else
                {
                    Debug.Log("Ended interaction with object.");
                }
            }

            // Debug.Log("isInteracting:"+isInteracting+" isInteractingWithShadows:"+isInteractingWithShadow);
            if (isInteracting && !isInteractingWithShadow) 
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    // disable interactable gameobject
                    interactableObj.GetComponent<InteractableObject>().enableInteraction = false;
                    isInteractingWithShadow = !isInteractingWithShadow;
                    smg.GetComponent<ShadowMeshGenerator>().ShadowCatch();
                    PlayShadowSound();
                }
            }
            else if (isInteracting && isInteractingWithShadow)
            {
                // Release gameobject
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    GameObject shadowMesh = GameObject.Find("Generated Shadow Mesh");
                    if (shadowMesh != null)
                    {
                        shadowMesh.GetComponent<Rigidbody>().isKinematic = false;
                        shadowMesh.GetComponent<InteractableObject>().enableInteraction = false;
                    }

                    isInteractingWithShadow = false;
              
                    // playerManager.GetComponent<MyPlayer>().CameraFollowPoint = GameObject.Find("CameraFollowPoint").transform;
                    playerManager.GetComponent<MyPlayer>().CameraFollowPoint = followPosition.transform;
                    playerManager.GetComponent<MyPlayer>().CameraFollowPoint.rotation = Quaternion.Euler(0, 225, 0);
                    playerManager.GetComponent<MyPlayer>().OrbitCamera.SetFollowTransform(playerManager.GetComponent<MyPlayer>().CameraFollowPoint);

                }
                
                

            }
            
            if (isInteractingWithShadow)
            {
                // enable shadow gameobject to interactable
                interactableObj = GameObject.Find("Generated Shadow Mesh");
                if (interactableObj != null)
                {
                    interactableObj.GetComponent<InteractableObject>().enableInteraction = true;
                    playerManager.GetComponent<MyPlayer>().CameraFollowPoint = interactableObj.transform;
                    playerManager.GetComponent<MyPlayer>().OrbitCamera.SetFollowTransformInverseDirection(interactableObj.transform);
                }
            }

        }
        else
        {
            ui.SetActive(false);
        }
    }
    private void PlayShadowSound()
    {
        if (shadowSound != null)
        {
            shadowSound.Play();
        }
    }
}

