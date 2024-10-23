using UnityEngine;

public class Destroyer : MonoBehaviour
{
    private float dissolveSpeed = 1.0f;
    private float dissolveProgress = 0.0f;
    private bool isDissolving = false;
    private Material material;

    private void Start()
    {
        material = GetComponent<Renderer>().material;
    }

    void Update()
    {
        if (isDissolving)
        {
            dissolveProgress += Time.deltaTime * dissolveSpeed;
            material.SetFloat("_DissolveProgress", dissolveProgress);

            if (dissolveProgress >= 1.0f)
            {
                isDissolving = false;
                Destroy(gameObject);
            }
        }
    }

    public void StartDissolve()
    {
        isDissolving = true;
    }
}
