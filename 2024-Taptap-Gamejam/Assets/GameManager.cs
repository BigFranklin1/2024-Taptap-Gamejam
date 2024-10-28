using KinematicCharacterController.Walkthrough.Crouching;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Camera mainCam;
    
    [SerializeField]
    private Camera playerCam;
    [SerializeField]
    private MyPlayer player;

    [SerializeField]
    private Canvas startMenu;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        playerCam.gameObject.SetActive(false);
        mainCam.gameObject.SetActive(true);

    }

    public void OnStartClick()
    {
        StartGame();
    }

    void StartGame()
    {
        print("start game");
        mainCam.gameObject.SetActive(false);
        startMenu.gameObject.SetActive(false);
        playerCam.gameObject.SetActive(true);

        player.StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
