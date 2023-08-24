using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Main.Player_Locomotion;
using Cinemachine;
// Manager of entire game format/structure 
// Makes sure each element is set in place the correct way
public class GM : MonoBehaviour
{
    private PC_3D_Controller PC;

    public bool currentlyUpgrading = false;

    [SerializeField] CinemachineVirtualCamera gameplayCamera, villageCamera, pivotCamera;
    [Header("Script Helpers")]
    public GameObject UpgradeHelper;
    public VillageItem currentSelectedVillagePiece;
    Vector3 pivotCamStartPos;
    #region Enable/Disable
    private void OnEnable()
    {
        CameraSwitcher.Register(gameplayCamera);
        CameraSwitcher.Register(villageCamera);
        CameraSwitcher.SwitchCamera(gameplayCamera);
    }

    private void OnDisable()
    {
        CameraSwitcher.UnRegister(gameplayCamera);
        CameraSwitcher.UnRegister(villageCamera);
    }
    #endregion
    private void Awake()
    {
        StartCoroutine(CameraChecker());
    }

    // Start is called before the first frame update
    void Start()
    {
        PC = GameObject.FindObjectOfType<PC_3D_Controller>();

        if (PC == null)
        {
            Debug.LogWarning("PC object not in the scene" + " Please add through assets");
        }
        else
            return;

        // Obtain start position of pivot camera 
        pivotCamStartPos = pivotCamera.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // checker for upgrading
        if(CameraSwitcher.IsCameraActive(villageCamera) | CameraSwitcher.IsCameraActive(pivotCamera))
        {
            currentlyUpgrading = true;
        }
        else
        {
            currentlyUpgrading=false;
            UpgradeHelper.GetComponent<UpgradeDetection_Helper>().hitMovingObject = null;
        }

        if (currentSelectedVillagePiece != null & CameraSwitcher.activeCamera == gameplayCamera)
        {
            currentSelectedVillagePiece.GetComponent<VillageItem>();
            StartCoroutine(currentSelectedVillagePiece.MovementDown());
        }
        // If the player enters the upgrade hall which for now is an Input of Spacebar

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if(CameraSwitcher.IsCameraActive(gameplayCamera))   // Am I currently In Game View?
            {
                // change the current camera to be the upgradecamera (Village Camera View)
                CameraSwitcher.SwitchCamera(villageCamera);
                // Turn off Player Input On PC 
                PC.GetComponent<PC_3D_Controller>().enabled = false;
                // Turn on my Camera animation handler - Of type Script
                UpgradeHelper.GetComponent<UpgradeDetection_Helper>().enabled = true;
                currentSelectedVillagePiece = null;
            }
            else if(CameraSwitcher.IsCameraActive(villageCamera))   // Am I currently using the Upgrade Village System?
            {
                // We have exited Upgrading lets go back to gameplay View
                CameraSwitcher.SwitchCamera(gameplayCamera);
                // Turn the player script back on to control again
                PC.GetComponent<PC_3D_Controller>().enabled=true;
                // we switched cameras so we no longer need this to be active
                UpgradeHelper.GetComponent<UpgradeDetection_Helper>().enabled = false;
            }
            else if (CameraSwitcher.IsCameraActive(gameplayCamera) | CameraSwitcher.IsCameraActive(villageCamera) & currentlyUpgrading) // The player decided to click an object
            {
                UpgradeHelper.GetComponent<UpgradeDetection_Helper>().enabled = false;
                CameraSwitcher.SwitchCamera(pivotCamera);
                PC.GetComponent<PC_3D_Controller>().enabled = false;
                Transform cameraParent = gameplayCamera.transform.parent.transform;

                CameraSwitcher.SwitchCamera(gameplayCamera);
                pivotCamera.transform.parent = cameraParent;
                pivotCamera.transform.position = pivotCamStartPos;
            }
        }
    }

    IEnumerator CameraChecker()
    {
        // Function which checks our camera vars are set correct before Start call
        // Local Var for getting value of the main camera 
        Camera Cam = Camera.main;
        // checking if the camera is in the right position as a child object of pivot parent
        if (Cam.transform.position.z != -10)
            // If camera itself  is not in the correct Z transform position of -10 then change value to correct position (Depth)
            Cam.transform.localPosition = new Vector3(0, 0, -10);
        // Check to see Projection mode is correct. If not then
        if (Cam.orthographic != true)
        {
            // Add error to console to alert dev of problem
            Debug.LogError("Wrong Camera Project Mode " +
    "please use orthographic " + "Switching Mode Now");
            // Change Projection to correct view for Isometric frmat 
            Cam.orthographic = true;
            // Wait time for the message to be read
            yield return new WaitForSeconds(5f);
            // cull void for clearing logs 
            clearLog();
        }

        else
            yield break;
    }

    // Helps keep console clear for debugging purposes only - allows log clearing on runtime just call the function when needed
    void clearLog()
    {
        // fill var to gain info about the UnityEditor 
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        // Get Console from Editor 
        var type = assembly.GetType("UnityEditor.LogEntries");
        // Get the clear function from console 
        var method = type.GetMethod("Clear");
        // invoke a clear 
        method.Invoke(new object(), null);
    }
}
