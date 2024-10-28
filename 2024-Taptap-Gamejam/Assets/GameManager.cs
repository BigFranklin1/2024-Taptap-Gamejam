using KinematicCharacterController.Walkthrough.Crouching;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Camera mainCam;

    [SerializeField]
    private MyPlayer player;

    [SerializeField]
    private Canvas startMenu;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
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
        player.StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}