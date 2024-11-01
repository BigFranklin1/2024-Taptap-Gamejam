using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowExtremeCaseUIHandler : MonoBehaviour
{
    private GameObject noShadowUI;
    private GameObject tooLargeUI;

    private GameObject currentActiveUI;

    void Start()
    {
        noShadowUI = transform.Find("NoShadow")?.gameObject;
        tooLargeUI = transform.Find("TooLarge")?.gameObject;

        noShadowUI.SetActive(false);
        tooLargeUI.SetActive(false);
    }

    public void SwitchUI(ShadowState state)
    {
        currentActiveUI?.SetActive(false);
        switch (state)
        {
            case ShadowState.None:
                noShadowUI?.SetActive(true);
                currentActiveUI = noShadowUI;
                break;
            case ShadowState.Oversize:
                tooLargeUI?.SetActive(true);
                currentActiveUI = tooLargeUI;
                break;
            case ShadowState.Normal:
                currentActiveUI = null;
                break;
        }
    }
}
