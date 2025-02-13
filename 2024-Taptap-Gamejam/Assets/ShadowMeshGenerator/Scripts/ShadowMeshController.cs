﻿using UnityEngine;
using UnityEngine.VFX;

public class ShadowMeshController : MonoBehaviour
{
    private Material material;
    #if UNITY_WEBGL
        private ParticleSystem particleComponent;
    #else
        private VisualEffect vfxComponent;
    #endif

    private bool isAppearing = false;
    private bool isEmissing = false;
    private float appearProgress = 0.0f;
    private float appearSpeed = 0.5f;
    private float emissionIntensity = 1.0f;
    // private float startTime;

    private bool isDestroyable = true;
    private bool isDestroying = false;
    private float destroySpeed = 1.0f;

    public void Appear(Mesh mesh)
    {
        #if UNITY_WEBGL
            GameObject particlePrefab = Resources.Load<GameObject>("VFX/AppearEffect_WebGL");
            GameObject particleInstance = Instantiate(particlePrefab, transform.position, Quaternion.identity);
            particleInstance.transform.SetParent(transform);
            particleComponent = particleInstance.GetComponent<ParticleSystem>();
            var shape = particleComponent.shape;
            shape.mesh = mesh;
            particleComponent.Play();
        #else
            VisualEffectAsset vfxGraph = Resources.Load<VisualEffectAsset>("VFX/AppearEffect");
            vfxComponent = gameObject.AddComponent<VisualEffect>();
            vfxComponent.visualEffectAsset = vfxGraph;
            vfxComponent.SetMesh("ShadowMesh", mesh);
            vfxComponent.Play();
        #endif

        material = new Material(Shader.Find("Unlit/ShadowMesh"));
        // material = new Material(Shader.Find("Shader Graphs/ShadowMesh_Appear"));
        gameObject.AddComponent<MeshRenderer>().material = material;
        material.SetTexture("_DissolveTex", Resources.Load<Texture2D>("Textures/noise"));
        isAppearing = true;
        isEmissing = true;
        // startTime = Time.time;
    }

    public void Destroy()
    {
        //material = new Material(Shader.Find("Shader Graphs/ShadowMesh_Destroy"));
        //gameObject.GetComponent<MeshRenderer>().material = material;
        if (isDestroyable)
        {
            isDestroying = true;
        }
    }

    //private float easeInOutCubic(float t)
    //{
    //    return 2 * (t < 0.5 ? 4 * t * t * t : Mathf.Pow(-2 * t + 2, 3) / 2);
    //}

    void Update()
    {
        if (isAppearing || isEmissing)
        {
            appearProgress += Time.deltaTime * appearSpeed;
            if (isAppearing)
            {
                material.SetFloat("_AppearProgress", appearProgress);
                if (appearProgress >= 1.0f)
                {
                    appearProgress = 1.0f;
                    isAppearing = false;
                }
            }

            if (appearProgress <= 1)
            {
                // emissionIntensity = 1.0f + 0.35f * easeInOutCubic(appearProgress / 2);
                emissionIntensity = 1.0f + Mathf.Sin(0.8f * Mathf.PI * appearProgress);
                material.SetFloat("_EmissionIntensity", emissionIntensity);
            }
            //else if (appearProgress <= 1.5f)
            //{
            //    // emissionIntensity = 1.0f + 0.35f * easeInOutCubic(appearProgress - 0.5f);
            //    material.SetFloat("_EmissionIntensity", emissionIntensity);
            //}
            else
            {
                // Debug.LogWarning(Time.time - startTime);
                material.SetFloat("_EmissionIntensity", 1.25f);
                isEmissing = false;
            }
        }
        else if (isDestroying)
        {
            appearProgress -= Time.deltaTime * destroySpeed;
            material.SetFloat("_AppearProgress", appearProgress);

            if (appearProgress <= 0.0f)
            {
                #if UNITY_WEBGL
                    particleComponent.Stop();
                #else
                    vfxComponent.Stop();
                #endif

                isDestroying = false;
                Destroy(gameObject);
            }
        }
    }

    public void DisableDestruction()
    {
        isDestroyable = false;
    }
}
