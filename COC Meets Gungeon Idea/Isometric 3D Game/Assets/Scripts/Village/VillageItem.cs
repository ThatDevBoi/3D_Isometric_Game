using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageItem : MonoBehaviour
{
    #region Radius Variables
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;
    [HideInInspector]
    public bool attackingUnite;
    public LayerMask targetmask;
    public LayerMask ObstacleMask;
    public List<Transform> visableTargets = new List<Transform>();

    public LineRenderer fovRadiusLineRenderer;
    public int numberOfSegments = 360;

    #endregion
    // VillageItem variables
    public Stats Stats;
    public GM gameManager;
    public CinemachineVirtualCamera pivotCamera;
    public GameObject ProjectilePrefab;
    public float arrowSpeed = 10f;
    public float fireInterval = 2f; // Time interval between arrow shots.
    Transform currentTarget;
    private float lastFireTime;

    Transform firePosition;

    public int HP;


    public float followSpeed = 5f; // Speed at which the camera follows the object
    public float orbitSpeed = 30f; // Speed of orbit in degrees per second

    [SerializeField] bool isOrbiting = false;


    private void Start()
    {

        GM.Instance.RegisterDefenseTransform(transform);
       
        // Initialize VillageItem-specific properties
        gameObject.name = Stats.Name;

        HP = Stats.HP;


        // Create and configure the LineRenderer for the FOV radius.
        fovRadiusLineRenderer = gameObject.AddComponent<LineRenderer>();
        fovRadiusLineRenderer.positionCount = numberOfSegments + 1;
        fovRadiusLineRenderer.useWorldSpace = false; // Set to false to work with local coordinates.
        fovRadiusLineRenderer.startWidth = 0.1f;
        fovRadiusLineRenderer.endWidth = 0.1f;
        fovRadiusLineRenderer.material = new Material(Shader.Find("Sprites/Default")); // You can change the material as needed.
        fovRadiusLineRenderer.startColor = Color.green; // Set the color of the FOV radius.
        fovRadiusLineRenderer.endColor = Color.green;

        firePosition = gameObject.transform.GetChild(0).transform.GetChild(3).GetComponent<Transform>();

        UpdateFovRadius();
    }

    private void Update()
    {
        FindVisibleTargets();
        UpdateFovRadius();

        if (currentTarget != null)
        {
            if (Time.time - lastFireTime >= fireInterval)
            {
                FireProjectileAtTarget(currentTarget);
                lastFireTime = Time.time;
            }
        }

        // replace later with effects or mesh split
        if (HP <= 0)
            Destroy(gameObject);

    }

    #region Field Of View Radius / Shooting Logic
    IEnumerator FindTargetWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    void FindVisibleTargets()
    {
        visableTargets.Clear();

        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetmask);

        foreach (Collider col in targetsInViewRadius)
        {
            Transform target = col.transform;
            Vector3 dirToTarget = (target.position - transform.position);
            float distanceToTarget = dirToTarget.magnitude;

            // Check if the target is within the FOV radius.
            if (distanceToTarget <= viewRadius)
            {
                // Check if there are obstacles between the archer and the target.
                if (!Physics.Raycast(transform.position, dirToTarget.normalized, distanceToTarget, ObstacleMask))
                {
                    visableTargets.Add(target);

                    // If we don't have a current target, set it to the first visible target.
                    if (currentTarget == null)
                    {
                        currentTarget = target;
                    }
                }
            }
        }
        // Check if the current target is no longer visible and remove it.
        if (currentTarget != null && !visableTargets.Contains(currentTarget))
        {
            currentTarget = null;
        }
    }

    void FireProjectileAtTarget(Transform target)
    {
        // Instantiate an arrow at the archer's position.
        GameObject arrow = Instantiate(ProjectilePrefab, firePosition.position, Quaternion.identity);

        // Calculate the direction from the archer to the target.
        Vector3 direction = (target.position - firePosition.position).normalized;

        // Apply velocity to the arrow to make it move toward the target.
        Rigidbody arrowRigidbody = arrow.GetComponent<Rigidbody>();
        if (arrowRigidbody != null)
        {
            arrowRigidbody.velocity = direction * arrowSpeed;
        }

        Destroy(arrow, 3f);
    }

    public Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    void UpdateFovRadius()
    {
        // Calculate the points for the FOV radius circle.
        float angleIncrement = 360f / numberOfSegments;
        for (int i = 0; i <= numberOfSegments; i++)
        {
            float angle = i * angleIncrement;
            Vector3 position = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * viewRadius, 0f, Mathf.Sin(angle * Mathf.Deg2Rad) * viewRadius);
            fovRadiusLineRenderer.SetPosition(i, position);
        }
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "AIProjectile")
        {
            SendMessage("ApplyDamage", other.GetComponent<AIProjectile>().DMG);
        }
    }

    private void ApplyDamage(int dmg)
    {
        HP -= dmg;
    }

    private void OnDestroy()
    {
        GM.Instance.UnregisterDefenseTransform(transform);
    }
}

[CreateAssetMenu(fileName = "NewScriptableObject", menuName = "Village/Village Piece/Stats")]
public class Stats : ScriptableObject
{
    public string Name = "";
    public int HP;
    public bool defenseItem = false;
    public float outputDMG;
}
