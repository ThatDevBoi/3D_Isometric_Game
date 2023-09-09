using UnityEngine;
using Cinemachine;
using System.Collections;

public class TestIdea : MonoBehaviour
{
    public Transform defenseObject; // The object to follow and orbit around
    public CinemachineVirtualCamera virtualCamera; // Reference to your Cinemachine Virtual Camera
    public float followSpeed = 5f; // Speed at which the camera follows the object
    public float orbitSpeed = 30f; // Speed of orbit in degrees per second

    private Transform virtualCameraTransform;

    private bool isOrbiting = false;

    private void Start()
    {
        if (virtualCamera != null)
        {
            virtualCameraTransform = virtualCamera.transform;
        }

        StartOrbit();
    }

    public void StartOrbit()
    {
        if (virtualCamera != null && defenseObject != null)
        {
            isOrbiting = true;
        }
    }

    public void StopOrbit()
    {
        if (virtualCamera != null)
        {
            isOrbiting = false;
        }
    }

    private void Update()
    {
        if (defenseObject != null && virtualCamera != null)
        {
            // Calculate the camera's position to smoothly follow the object
            Vector3 targetPosition = Vector3.Lerp(virtualCameraTransform.position, defenseObject.position, followSpeed * Time.deltaTime);
            virtualCameraTransform.position = targetPosition;

            // Rotate the virtual camera around the defense object
            virtualCameraTransform.RotateAround(defenseObject.position, Vector3.up, orbitSpeed * Time.deltaTime);
        }
    }


}
