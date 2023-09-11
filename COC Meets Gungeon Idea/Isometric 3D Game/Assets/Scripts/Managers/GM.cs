using Cinemachine;
using Main.Player_Locomotion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using UnityEngine.EventSystems;

// Manager of entire game format/structure
// Makes sure each element is set in place the correct way
public class GM : MonoBehaviour
{
    public static GM Instance { get; private set; } // Singleton
    private List<AI_Unit> aiUnits = new List<AI_Unit>();    // List of All deployed AI Troops
    private List<Transform> defenseTransforms = new List<Transform>();  // List of all Defence Towers
    // Player Controller Class Reference
    private PC_3D_Controller PC;
    [Tooltip("Flag to check if the player is inside the Uogade Menu")]
    public bool currentlyUpgrading = false;
    [Tooltip("VCamera References to all Cameras that change on Runtime")]
    public CinemachineVirtualCamera gameplayCamera, villageCamera, pivotCamera;

    [Header("Script Helpers")]
    public UpgradeDetection_Helper UpgradeHelper;
    public VillageItem currentSelectedVillagePiece;

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

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PC = FindObjectOfType<PC_3D_Controller>();
        UpgradeHelper = FindObjectOfType<UpgradeDetection_Helper>();

        if (PC == null)
        {
            Debug.LogWarning("PC object not in the scene" + " Please add through assets");
        }
        else
            return;
    }

    void Update()
    {
        if (CameraSwitcher.IsCameraActive(gameplayCamera))
            UpgradeHelper.GetComponent<UpgradeDetection_Helper>().currentState = UpgradeDetection_Helper.DefenseState.None;
            

        if (CameraSwitcher.IsCameraActive(villageCamera) || CameraSwitcher.IsCameraActive(pivotCamera))
        {
            currentlyUpgrading = true;
        }
        else
        {
            currentlyUpgrading = false;

        }

        if (CameraSwitcher.IsCameraActive(pivotCamera) && Input.GetKey(KeyCode.Escape) &
            UpgradeHelper.currentState == UpgradeDetection_Helper.DefenseState.Orbitting & UpgradeHelper.cameraBrain.IsBlending == false)
        {
            ExitUpgrade(); // -> Remove 
            UpgradeHelper.GetComponent<UpgradeDetection_Helper>().orbitingIsDone = true;


        }
        if (CameraSwitcher.IsCameraActive(villageCamera))
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                PC.isMovingTowardsTownHall = false;
                PC.isMovingAwayFromTownHall = true;
                CameraSwitcher.SwitchCamera(gameplayCamera);
                UpgradeHelper.GetComponent<UpgradeDetection_Helper>().enabled = false;
            }
        }
    }

    IEnumerator CameraChecker()
    {
        Camera Cam = Camera.main;

        if (Cam.transform.position.z != -10)
            Cam.transform.localPosition = new Vector3(0, 0, -10);

        if (Cam.orthographic != true)
        {
            Debug.LogError("Wrong Camera Project Mode " + "please use orthographic " + "Switching Mode Now");
            Cam.orthographic = true;
            yield return new WaitForSeconds(5f);
            ClearLog();
        }
        else
            yield break;
    }

    void ClearLog()
    {
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    public void RegisterAIUnit(AI_Unit aiUnit)
    {
        if (!aiUnits.Contains(aiUnit))
        {
            aiUnits.Add(aiUnit);
        }
    }

    public void UnregisterAIUnit(AI_Unit aiUnit)
    {
        aiUnits.Remove(aiUnit);
    }

    public void RegisterDefenseTransform(Transform defenseTransform)
    {
        if (!defenseTransforms.Contains(defenseTransform))
        {
            defenseTransforms.Add(defenseTransform);
        }
    }

    public void UnregisterDefenseTransform(Transform defenseTransform)
    {
        defenseTransforms.Remove(defenseTransform);
    }

    public Transform GetNearestDefense(Vector3 position)
    {
        Transform nearestTarget = null;
        float distance = Mathf.Infinity;

        foreach (Transform defenseTransform in defenseTransforms)
        {
            float dist = Vector3.Distance(position, defenseTransform.position);
            if (dist < distance)
            {
                nearestTarget = defenseTransform;
                distance = dist;
            }
        }

        return nearestTarget;
    }

    void ExitUpgrade()
    {
        UpgradeHelper.GetComponent<UpgradeDetection_Helper>().enabled = true;
        currentSelectedVillagePiece = null;

    }
}
