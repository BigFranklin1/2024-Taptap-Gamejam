using UnityEngine;
using System.Collections.Generic;

public class PlatformTrigger : MonoBehaviour
{
    private static PlatformTrigger currentClosestTrigger = null;
    private static GameObject playerGO = null;
    private static List<PlatformTrigger> activeTriggers = new List<PlatformTrigger>();

    public bool triggered = false;

    private void Start()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerGO == null)
        {
            playerGO = other.gameObject;
        }
        if (other.CompareTag("Player"))
        {
            if (!activeTriggers.Contains(this))
            {
                activeTriggers.Add(this);
                if (activeTriggers.Count == 1)
                {
                    TriggerPlatform();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            triggered = false;
            activeTriggers.Remove(this);
            if (currentClosestTrigger == this)
            {
                currentClosestTrigger = null;
            }
        }
    }

    private void Update()
    {
        if (activeTriggers.Count > 1)
        {
            PlatformTrigger closestTrigger = null;
            float minDistance = float.MaxValue;

            Vector3 playerPosition = playerGO.transform.position;
            foreach (var trigger in activeTriggers)
            {
                Vector3 platformPosition = trigger.transform.position;
                float distance = Vector2.Distance(new Vector2(platformPosition.x, platformPosition.z), new Vector2(playerPosition.x, playerPosition.z));
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTrigger = trigger;
                }
            }

            if (currentClosestTrigger != closestTrigger)
            {
                closestTrigger.TriggerPlatform();
            }
        }
    }

    private void TriggerPlatform()
    {
        triggered = true;
        currentClosestTrigger = this;
    }
}
