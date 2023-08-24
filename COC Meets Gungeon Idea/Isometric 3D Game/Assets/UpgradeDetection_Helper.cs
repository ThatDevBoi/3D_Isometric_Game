using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeDetection_Helper : MonoBehaviour
{
    public Camera VilOverviewCam;
    [SerializeField]
    Transform hitObjectCameraPivoter;  // Obtained during runtime
    public GameObject hitMovingObject;
    public CinemachineVirtualCamera pivotCam;   // Apply manually until finished 
    public GM gameManager;

    public float movementSpeedToUpgradeView = 2;
    public float desiredHeightToUpgradeView = 2.5f;

    public bool selectedPiece = false;
    public Vector3 startPosition = Vector3.zero;
    public Vector3 endPosition = Vector3.zero;
    [HideInInspector]
    public Transform interactableObject;   // Object we move during animation 

    public LayerMask CanHit;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindFirstObjectByType<GM>() as GM;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastClickDetection();

    }

    void RaycastClickDetection()
    {
        if(Input.GetMouseButtonDown(0) /*&& !EventSystem.current.IsPointerOverGameObject()*/)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, CanHit) && hitMovingObject == null)
            {
                hitMovingObject = hit.transform.gameObject;
                //Debug.Log(hit.collider.name);
                //startPosition = hit.transform.position;

                //interactableObject = hit.transform;

                //interactableRigibody = interactableObject.GetComponent<Rigidbody>();
                //selectedPiece = true;

                hit.collider.transform.gameObject.GetComponent<VillageItem>().StartMoving();

                gameManager.currentSelectedVillagePiece = hit.transform.gameObject.GetComponent<VillageItem>();

                CameraSettings(hit);

            }
        }

    }

    void CameraSettings(RaycastHit hit)
    {
        // Obtain the Pivot Holder
        hitObjectCameraPivoter = hit.collider.gameObject.transform.GetChild(0).GetComponent<Transform>();
        // set up the camera vars 
        pivotCam.LookAt = hit.collider.transform;
        pivotCam.transform.parent = hitObjectCameraPivoter.transform;   // child cam to pivot point 
        pivotCam.enabled = true;
    }
}
