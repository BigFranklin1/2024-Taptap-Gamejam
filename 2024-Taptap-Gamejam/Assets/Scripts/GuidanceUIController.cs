using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GuidanceUIState
{
    BeforeInteraction,
    ControlObject,
    ControlShadow,
    Nothing
}

public class GuidanceUIController : MonoBehaviour
{
    public GuidanceUIState currentState;

    private GameObject beforeInteractionUI;
    private GameObject controlObjectUI;
    private GameObject controlShadowUI;

    private GameObject currentActiveUI;

    void Start()
    {
        beforeInteractionUI = transform.Find("BeforeInteraction")?.gameObject;
        controlObjectUI = transform.Find("ControlObject")?.gameObject;
        controlShadowUI = transform.Find("ControlShadow")?.gameObject;

        beforeInteractionUI.SetActive(false);
        controlShadowUI.SetActive(false);
        controlObjectUI.SetActive(false);
        currentState = GuidanceUIState.Nothing;
    }

    public void SwitchUI(GuidanceUIState state)
    {
        if (currentState != state)
        {
            currentActiveUI?.SetActive(false);
            currentState = state;
            switch (state)
            {
                case GuidanceUIState.BeforeInteraction:
                    beforeInteractionUI.SetActive(true);
                    currentActiveUI = beforeInteractionUI;
                    break;
                case GuidanceUIState.ControlObject:
                    controlObjectUI.SetActive(true);
                    currentActiveUI = controlObjectUI;
                    break;
                case GuidanceUIState.ControlShadow:
                    controlShadowUI.SetActive(true);
                    currentActiveUI = controlShadowUI;
                    break;
                case GuidanceUIState.Nothing:
                    currentActiveUI = null;
                    break;
            }
        }
        
    }
}
