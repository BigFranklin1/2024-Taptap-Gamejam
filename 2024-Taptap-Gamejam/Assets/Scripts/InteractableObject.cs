using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public float rotationSpeed = 100f; // 控制旋转速度
    public float moveSpeed = 5f;       // 控制移动速度

    public bool enableInteraction = false; // 是否可以和物体进行交互
    private bool isDragging = false;   // 标记是否正在拖动物体
    private Vector3 lastMousePosition; // 记录上一次的鼠标位置

    // Update is called once per frame
    void Update()
    {
        if(enableInteraction){
            HandleRotation();
            HandleMovement();
        }
    }

    // 处理对象旋转
    //void HandleRotation()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        isDragging = true;
    //        lastMousePosition = Input.mousePosition;
    //    }

    //    if (Input.GetMouseButtonUp(0))
    //    {
    //        isDragging = false;
    //    }

    //    if (isDragging)
    //    {
    //        // 获取当前鼠标位置和上一次鼠标位置之间的差值
    //        Vector3 deltaMouse = Input.mousePosition - lastMousePosition;

    //        // X 轴旋转 (上下移动控制绕 X 轴旋转)
    //        float rotationX = deltaMouse.y * rotationSpeed * Time.deltaTime;

    //        // Y 轴旋转 (左右移动控制绕 Y 轴旋转)
    //        float rotationY = -deltaMouse.x * rotationSpeed * Time.deltaTime;

    //        // 旋转物体（限制 Z 轴旋转，防止翻转）
    //        transform.rotation *= Quaternion.Euler(rotationX, rotationY, 0);

    //        // 更新最后的鼠标位置
    //        lastMousePosition = Input.mousePosition;
    //    }
    //}

    void HandleRotation()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            // 获取当前鼠标位置和上一次鼠标位置之间的差值
            Vector3 deltaMouse = Input.mousePosition - lastMousePosition;

            // X 轴旋转 (上下移动控制绕 X 轴旋转)
            float rotationX = deltaMouse.y * rotationSpeed * Time.deltaTime;

            // Y 轴旋转 (左右移动控制绕 Y 轴旋转)
            float rotationY = -deltaMouse.x * rotationSpeed * Time.deltaTime;

            // 旋转物体（限制 Z 轴旋转，防止翻转）
            transform.rotation *= Quaternion.Euler(rotationX, rotationY, 0);

            // 更新最后的鼠标位置
            lastMousePosition = Input.mousePosition;
        }
    }

    // 处理对象位移
    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveY = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        // 初始化 Y 轴的移动量
        float moveZ = 0;

        // 通过 Q 和 E 键控制 Y 轴的运动
        if (Input.GetKey(KeyCode.Q))
        {
            moveZ = moveSpeed * Time.deltaTime; // Q 键使物体上升
        }
        else if (Input.GetKey(KeyCode.E))
        {
            moveZ = -moveSpeed * Time.deltaTime; // E 键使物体下降
        }

        transform.Translate(new Vector3(-moveX, moveZ, -moveY), Space.World);
    }
}
