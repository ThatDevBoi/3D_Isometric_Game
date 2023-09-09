using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeDetection_Helper : MonoBehaviour
{
    public Camera VilOverviewCam;
    [SerializeField]
    Transform hitObjectCameraPivoter;
    public GameObject hitMovingObject;
    public CinemachineVirtualCamera pivotCam;
    public GM gameManager;

    public float movementSpeedToUpgradeView = 2;
    public float desiredHeightToUpgradeView = 2.5f;

    public bool selectedPiece = false;
    public Vector3 startPosition = Vector3.zero;
    public Vector3 endPosition = Vector3.zero;
    [HideInInspector]
    public Transform interactableObject;

    public LayerMask CanHit;

    void Start()
    {
        gameManager = FindObjectOfType<GM>();
    }

    void Update()
    {
        RaycastClickDetection();
    }

    void RaycastClickDetection()
    {
        if (Input.GetMouseButtonDown(0) && gameManager.currentlyUpgrading)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, CanHit) && hitMovingObject == null)
            {
                hitMovingObject = hit.transform.gameObject;
                CameraSwitcher.SwitchCamera(pivotCam);
                hit.collider.transform.gameObject.GetComponent<VillageItem>().StartMoving();
                gameManager.currentSelectedVillagePiece = hit.transform.gameObject.GetComponent<VillageItem>();
                CameraSettings(hit);
            }
        }
    }

    void CameraSettings(RaycastHit hit)
    {
        hitObjectCameraPivoter = hit.collider.gameObject.transform.GetChild(0).GetChild(4).GetComponent<Transform>();
        pivotCam.LookAt = hitObjectCameraPivoter;
        pivotCam.transform.parent = hitObjectCameraPivoter.transform;
        pivotCam.enabled = true;
    }
}