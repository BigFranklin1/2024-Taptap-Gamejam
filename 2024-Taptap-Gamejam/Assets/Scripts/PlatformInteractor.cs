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
    public float interactionRange = 5f; // Range within which interaction is possible
    private bool isInteracting = false; // Track whether the player is currently interacting

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
        }
        else
        {
            ui.SetActive(false);
        }
    }
}

