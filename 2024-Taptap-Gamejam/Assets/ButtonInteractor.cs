using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonInteractor : MonoBehaviour
{
    // 需要的组件
    public AudioSource buttonSound;      // 按钮触发音效
    public GameObject targetBlock;       // 目标方块
    public Vector3 targetPosition;       // 方块的目标位置
    public float moveSpeed = 2.0f;       // 方块移动的速度

    private bool isActivated = false;    // 按钮是否被触发

    // 当玩家触碰到按钮时触发
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            isActivated = true;
            PlayButtonSound();
            
            // enable platform
            targetBlock.GetComponent<PlatformInteractor>().isEnabled = true;
            StartCoroutine(MoveBlock());
        }
    }

    // 播放按钮的音效
    private void PlayButtonSound()
    {
        if (buttonSound != null)
        {
            buttonSound.Play();
        }
    }

    // 移动方块到指定位置
    private IEnumerator MoveBlock()
    {
        while (Vector3.Distance(targetBlock.transform.position, targetPosition) > 0.01f)
        {
            targetBlock.transform.position = Vector3.MoveTowards(targetBlock.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        targetBlock.transform.position = targetPosition; // 确保到达目标位置
    }
}
