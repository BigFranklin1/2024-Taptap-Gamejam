using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Camera mainCam;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnStartClick()
    {
        StartGame();
    }

    void StartGame()
    {
        print("start game");

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
