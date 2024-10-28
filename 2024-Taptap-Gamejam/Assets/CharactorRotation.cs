using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController.Walkthrough.Crouching;
using UnityEngine;

public class CharactorRotation : MonoBehaviour
{
    public float moveSpeed = 5f;
    public MyPlayer player;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

    }

    void Update()
    {
        Move();
        Animate();
    }

    void Move()
    {
        float moveHorizontal = Input.GetAxis("Horizontal"); // A/D 或 左/右箭头
        float moveVertical = Input.GetAxis("Vertical"); // W/S 或 上/下箭头

        Vector3 movement = new Vector3(-moveHorizontal, 0.0f, -moveVertical);

        if (movement != Vector3.zero && player.enableInteraction)
        {
            // 计算旋转方向
            Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 720 * Time.deltaTime);
        }
    }

    void Animate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal"); // A/D 或 左/右箭头
        float moveVertical = Input.GetAxis("Vertical"); // W/S 或 上/下箭头

        // 判断是否有输入
        bool isRunning = moveHorizontal != 0 || moveVertical != 0;

        // 设置动画参数
        animator.SetBool("isRunning", isRunning);
    }
}
