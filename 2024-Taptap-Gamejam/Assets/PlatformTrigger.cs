using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlatformTrigger : MonoBehaviour
{
    private static PlatformTrigger firstInstance = null;

    private static PlatformTrigger currentClosestTrigger = null;
    private static GameObject playerGO = null;
    private static List<PlatformTrigger> activeTriggers = new List<PlatformTrigger>();

    public GameObject shadowCaster;
    public bool triggered = false;
    // private Material shadowCasterMaterial;

    private void Awake()
    {
        if (firstInstance == null)
        {
            firstInstance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnDestroy()
    {
        if (firstInstance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            firstInstance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentClosestTrigger = null;
        playerGO = null;
        activeTriggers.Clear();
    }

    private void Start()
    {
    //    if (shadowCaster != null)
    //    {
    //        shadowCasterMaterial = shadowCaster.GetComponent<Renderer>().material;
    //    }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("Enter " + gameObject.name);
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
        // Debug.Log("Exit " + gameObject.name);
        if (other.CompareTag("Player"))
        {
            activeTriggers.Remove(this);
            if (currentClosestTrigger == this)
            {
                UntriggerPlatform();
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
                // Debug.Log("Switch");
                currentClosestTrigger.UntriggerPlatform();
                closestTrigger.TriggerPlatform();
            }
        }
        else if (activeTriggers.Contains(this) && currentClosestTrigger != this)
        {
            TriggerPlatform();
        }
    }

    private void TriggerPlatform()
    {
        // Debug.Log("Trigger "+gameObject.name + "    Count " + activeTriggers.Count);
        triggered = true;
        currentClosestTrigger = this;
        // shadowCasterMaterial.SetInt("_StencilRef", 0);
        shadowCaster.layer = LayerMask.NameToLayer("InteractableShadowCasters");
    }

    private void UntriggerPlatform()
    {
        // Debug.Log("Untrigger " + gameObject.name + "    Count " + activeTriggers.Count);
        triggered = false;
        currentClosestTrigger = null;
        // shadowCasterMaterial.SetInt("_StencilRef", -1);
        shadowCaster.layer = LayerMask.NameToLayer("Default");
    }
}
