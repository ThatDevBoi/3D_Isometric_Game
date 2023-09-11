using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeDetection_Helper : MonoBehaviour
{
    public CinemachineVirtualCamera pivotCam;
    public GM gameManager;
    public float movementSpeedToUpgradeView = 2;
    public LayerMask CanHit;

    public enum DefenseState
    {
        None,
        ChangingToPivotCam,
        FollowingDefenceUp,
        Orbitting,
        FollowingDefenceDownAndLerping
    }

    public DefenseState currentState;
    private Vector3 cameraOriginalPosition;
    private Quaternion cameraOriginalRotation;
    private Vector3 targetUpPosition;
    private Vector3 targetDownPosition;
    public float desiredHeight;
    public Transform defenseTransform;
    public bool orbitingIsDone;
    private float lerpSpeed = 0.4f; // Found this is the best Value so there is no Jitter
    private float startTime;
    private float journeyLength;

    public CinemachineBrain cameraBrain;
    public float orbitSpeed;


    void Start()
    {
        gameManager = FindObjectOfType<GM>();

        currentState = DefenseState.None;

        startTime = Time.time;

    }

    void Update()
    {
        RaycastClickDetection();
        #region Camera Behaviour Enums
        switch (currentState)
        {
            case DefenseState.None:
                //return;
                orbitingIsDone = false;
                break;

            case DefenseState.ChangingToPivotCam:
                HandleChangingToPivotCam();
                break;

            case DefenseState.FollowingDefenceUp:
                HandleFollowingDefenceUp();
                break;

            case DefenseState.Orbitting:
                HandleOrbitting();
                break;

            case DefenseState.FollowingDefenceDownAndLerping:
                HandleFollowingDefenceAndLerping();
                break;

            default:
                break;
        }
        #endregion
    }

    // New 
    private void HandleChangingToPivotCam()
    {
        CameraSwitcher.SwitchCamera(pivotCam);
        pivotCam.LookAt = defenseTransform;
        pivotCam.Follow = defenseTransform;
        cameraOriginalPosition = pivotCam.transform.position;
        cameraOriginalRotation = pivotCam.transform.rotation;

        if(cameraBrain.IsBlending == false)
        {
            currentState = DefenseState.FollowingDefenceUp;
        }
    }

    private void HandleFollowingDefenceUp()
    {
        float distanceCovered = (Time.time - startTime) * movementSpeedToUpgradeView;
        float fractionOfJourney = distanceCovered / journeyLength;
        defenseTransform.position = Vector3.Lerp(defenseTransform.position, targetUpPosition, fractionOfJourney);

        if (Vector3.Distance(defenseTransform.position, targetUpPosition) < 0.01f)
        {
            currentState = DefenseState.Orbitting;
        }
    }

    private void HandleOrbitting()
    {
        pivotCam.transform.RotateAround(gameObject.transform.position, Vector3.up, orbitSpeed * Time.deltaTime);

        if (orbitingIsDone)
        {
            currentState = DefenseState.FollowingDefenceDownAndLerping;
        }
    }

    private void HandleFollowingDefenceAndLerping()
    {
        float distanceCovered = (Time.time - startTime) * lerpSpeed;
        float fractionOfJourney = distanceCovered / journeyLength;
        defenseTransform.position = Vector3.Lerp(defenseTransform.position, targetDownPosition, fractionOfJourney);
        pivotCam.transform.position = Vector3.Lerp(pivotCam.transform.position, cameraOriginalPosition, fractionOfJourney);
        pivotCam.transform.rotation = Quaternion.Slerp(pivotCam.transform.rotation, cameraOriginalRotation, fractionOfJourney);
        if (Vector3.Distance(defenseTransform.position, targetDownPosition) < 0.01f)
        {
            CameraSwitcher.SwitchCamera(gameManager.villageCamera);

            orbitingIsDone = false;
            currentState = DefenseState.None;
            defenseTransform = null;
        }
    }

    // Function to start the defense behavior
    private void StartDefenseBehavior(Transform targetTransform, RaycastHit hit)
    {
        gameManager.currentSelectedVillagePiece = targetTransform.GetComponent<VillageItem>();
        pivotCam.LookAt = targetTransform;
        pivotCam.Follow = targetTransform;
        targetUpPosition = targetTransform.position + Vector3.up * desiredHeight;

        // Calculate targetDownPosition based on targetUpPosition and desiredHeight
        targetDownPosition = targetUpPosition - Vector3.up * desiredHeight;

        startTime = Time.time;
        defenseTransform = hit.transform;
        journeyLength = Vector3.Distance(defenseTransform.position, targetUpPosition);
        currentState = DefenseState.ChangingToPivotCam;
    }

    void RaycastClickDetection()
    {
        if (Input.GetMouseButtonDown(0) && gameManager.currentlyUpgrading)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, CanHit) && defenseTransform == null)
            {
                pivotCam.LookAt = hit.transform;
                pivotCam.Follow = hit.transform;
                StartDefenseBehavior(hit.transform, hit);
                gameManager.currentSelectedVillagePiece = hit.transform.gameObject.GetComponent<VillageItem>();
            }
        }
    }
}