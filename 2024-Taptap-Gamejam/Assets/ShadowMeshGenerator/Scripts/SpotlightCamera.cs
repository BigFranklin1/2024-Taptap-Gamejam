using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SpotlightCameraController : MonoBehaviour
{
    private Light spotlight;
    private Camera spotlightCamera;

    void Start()
    {
        spotlightCamera = GetComponent<Camera>();

        spotlight = GetComponentInParent<Light>();

        if (spotlight == null || spotlight.type != LightType.Spot)
        {
            Debug.LogError("No valid spotlight found in the parent. Please ensure the parent has a Spot Light.");
            return;
        }

        UpdateCameraTransform();
    }

    void Update()
    {
        UpdateCameraTransform();
    }

    private void UpdateCameraTransform()
    {
        transform.forward = spotlight.transform.forward;

        Vector3 right = Vector3.Cross(transform.forward, Vector3.up).normalized;

        Vector3 up = Vector3.Cross(right, transform.forward);

        transform.rotation = Quaternion.LookRotation(transform.forward, up);

        spotlightCamera.fieldOfView = spotlight.spotAngle;
    }
}
