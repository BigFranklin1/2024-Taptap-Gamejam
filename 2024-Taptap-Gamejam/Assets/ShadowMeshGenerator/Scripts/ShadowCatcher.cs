using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ShadowCatcher : MonoBehaviour
{
    public UniversalRendererData RPAsset;
    //public GameObject GameObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        // 按下E键并捕捉阴影
        if (Input.GetKeyDown(KeyCode.E))
        {
            EnableShadowCatcher();
        }
    }

    void EnableShadowCatcher()
    {
        if (RPAsset != null)
        {
            // 遍历 renderer features，找到 ShadowMaskRenderFeature
            foreach (var feature in RPAsset.rendererFeatures)
            {
                if (feature is ShadowMaskRenderFeature shadowMaskFeature)
                {
                    shadowMaskFeature.EnableShadowCatching();
                }
            }
        }
    }
}
