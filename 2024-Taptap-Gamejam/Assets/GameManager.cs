using KinematicCharacterController.Walkthrough.Crouching;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void OnExitClick()
    {
        ExitGame();
    }

    void StartGame()
    {
        print("start game");
        mainCam.gameObject.SetActive(false);
        startMenu.gameObject.SetActive(false);
        playerCam.gameObject.SetActive(true);

        player.StartGame();
    }

    void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #elif UNITY_WEBGL
            ReloadCurrentScene();
        #else
            Application.Quit();
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
