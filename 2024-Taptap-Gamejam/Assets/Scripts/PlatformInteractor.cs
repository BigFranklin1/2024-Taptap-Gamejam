using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using KinematicCharacterController.Walkthrough.Crouching;
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
    void Update()
    {
        // Check if the player is near the interactable object
        float distanceToInteractable = Vector3.Distance(playerObj.transform.position, transform.position);
        if (distanceToInteractable <= interactionRange)
        {
            ui.SetActive(true);
            // If player is within range and presses the 'E' key
            if (Input.GetKeyDown(KeyCode.Q))
            {
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

            if (isInteracting || isInteractingWithShadow) 
            {
                if (Input.GetKeyDown(KeyCode.E)) 
                {
                    // disable interactable gameobject
                    interactableObj.GetComponent<InteractableObject>().enableInteraction = false;
                    isInteractingWithShadow = !isInteractingWithShadow;
                    smg.GetComponent<ShadowMeshGenerator>().ShadowCatch();
                }
            }

            if (isInteractingWithShadow)
            {
                // enable shadow gameobject to interactable
                interactableObj = GameObject.Find("Generated Shadow Mesh");
                interactableObj.GetComponent<InteractableObject>().enableInteraction = true;
            }
        }
        else
        {
            ui.SetActive(false);
        }
    }
}

