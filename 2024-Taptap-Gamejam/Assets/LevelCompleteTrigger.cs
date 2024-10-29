using UnityEngine;
using KinematicCharacterController.Walkthrough.Crouching;

public class LevelCompleteTrigger : MonoBehaviour
{
    public GameObject gameCompleteUI;
    public GameObject playerManager; // 暂时disable interaction做的暂停
    public GameObject character;
    // 可以在这里定义通关后的行为，比如加载下一个场景或显示通关信息
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // 确保碰撞的对象是玩家
        {
            // 处理通关逻辑，比如加载下一个场景
            Debug.Log("通关！");
            gameCompleteUI.SetActive(true);
            playerManager.GetComponent<MyPlayer>().enableInteraction = false;
            character.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
