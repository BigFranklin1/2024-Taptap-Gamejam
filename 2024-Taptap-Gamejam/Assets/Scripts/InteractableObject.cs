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
            Vector3 deltaMouse = Input.mousePosition - lastMousePosition;
            float rotationX = deltaMouse.y * rotationSpeed * Time.deltaTime; // 根据鼠标y轴旋转对象的x轴
            float rotationY = -deltaMouse.x * rotationSpeed * Time.deltaTime; // 根据鼠标x轴旋转对象的y轴

            transform.Rotate(Vector3.up, rotationY, Space.World);
            transform.Rotate(Vector3.right, rotationX, Space.World);

            lastMousePosition = Input.mousePosition;
        }
    }

    // 处理对象位移
    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveY = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        transform.Translate(new Vector3(moveX, 0, moveY), Space.World);
    }
}
