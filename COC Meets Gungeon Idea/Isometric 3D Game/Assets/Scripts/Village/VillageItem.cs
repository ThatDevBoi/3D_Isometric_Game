using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class VillageItem : MonoBehaviour
{
    #region Radius Variables

    // View radius properties
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;
    [HideInInspector]
    public bool attackingUnite;
    public LayerMask targetmask;
    public LayerMask ObstacleMask;
    public List<Transform> visableTargets = new List<Transform>();

    // FOV LineRenderer properties
    public LineRenderer fovRadiusLineRenderer;
    public int numberOfSegments = 360;

    #endregion

    // VillageItem variables
    public Stats Stats;
    public GM gameManager;
    public CinemachineVirtualCamera pivotCamera;
    public GameObject ProjectilePrefab;
    public float arrowSpeed = 10f;
    public float fireInterval = 2f;
    Transform currentTarget;
    private float lastFireTime;

    Transform firePosition;

    public int HP;

    // Camera following properties
    public float followSpeed = 5f;
    public float orbitSpeed = 30f;
    [SerializeField] bool isOrbiting = false;

    // Grid snapping properties
    public float cellSize = 1.0f;

    private void Start()
    {
        // Register defense transform with game manager
        GM.Instance.RegisterDefenseTransform(transform);

        // Initialize VillageItem-specific properties
        gameObject.name = Stats.Name;
        HP = Stats.HP;

        // Create and configure the LineRenderer for the FOV radius.
        SetUpFOVLineRenderer();

        // Set fire position
        firePosition = gameObject.transform.GetChild(0).transform.GetChild(3).GetComponent<Transform>();

        // Update FOV radius
        UpdateFovRadius();
    }

    void SetUpFOVLineRenderer()
    {
        // Create and configure the LineRenderer for the FOV radius.
        fovRadiusLineRenderer = gameObject.AddComponent<LineRenderer>();
        fovRadiusLineRenderer.positionCount = numberOfSegments + 1;
        fovRadiusLineRenderer.useWorldSpace = false; // Set to false to work with local coordinates.
        fovRadiusLineRenderer.startWidth = 0.1f;
        fovRadiusLineRenderer.endWidth = 0.1f;
        fovRadiusLineRenderer.material = new Material(Shader.Find("Sprites/Default")); // You can change the material as needed.
        fovRadiusLineRenderer.startColor = Color.green; // Set the color of the FOV radius.
        fovRadiusLineRenderer.endColor = Color.green;
    }

    private void Update()
    {
        // Find visible targets and update FOV radius
        FindVisibleTargets();
        UpdateFovRadius();

        // Fire projectile at target if there is a valid target and enough time has passed
        if (currentTarget != null && Time.time - lastFireTime >= fireInterval)
        {
            FireProjectileAtTarget(currentTarget);
            lastFireTime = Time.time;
        }

        // Destroy object if HP is zero
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

            // Check if the target is within the FOV radius and no obstacles in between
            if (distanceToTarget <= viewRadius && !Physics.Raycast(transform.position, dirToTarget.normalized, distanceToTarget, ObstacleMask))
            {
                visableTargets.Add(target);

                // Set current target if not already set
                if (currentTarget == null)
                    currentTarget = target;
            }
        }

        // Remove current target if no longer visible
        if (currentTarget != null && !visableTargets.Contains(currentTarget))
            currentTarget = null;
    }

    void FireProjectileAtTarget(Transform target)
    {
        // Instantiate arrow at archer's position
        GameObject arrow = Instantiate(ProjectilePrefab, firePosition.position, Quaternion.identity);

        // Calculate direction to the target
        Vector3 direction = (target.position - firePosition.position).normalized;

        // Apply velocity to the arrow
        Rigidbody arrowRigidbody = arrow.GetComponent<Rigidbody>();
        if (arrowRigidbody != null)
            arrowRigidbody.velocity = direction * arrowSpeed;

        // Destroy arrow after a certain time
        Destroy(arrow, 3f);
    }

    public Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
            angleInDegrees += transform.eulerAngles.y;

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    void UpdateFovRadius()
    {
        // Calculate points for the FOV radius circle
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
        // Apply damage if collided with AI projectile
        if (other.gameObject.tag == "AIProjectile")
        {
            SendMessage("ApplyDamage", other.GetComponent<AIDamage>().DMG);
        }

        if(other.gameObject.tag == "MeleeAttack")
        {
            SendMessage("ApplyDamage", other.GetComponent<AIDamage>().DMG);

        }
    }

    private void OnMouseDrag()
    {
        // Raycast to the ground to find the position to snap to
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground")))
        {
            // Snap the object to the nearest grid position
            Vector3 snapPosition = SnapToGrid(hit.point);
            transform.position = snapPosition;
        }
    }

    private Vector3 SnapToGrid(Vector3 position)
    {
        // Calculate the grid cell position based on the IsometricGridManager's cellSize
        float snappedX = Mathf.Floor(position.x / cellSize) * cellSize + cellSize / 2f;
        float snappedZ = Mathf.Floor(position.z / cellSize) * cellSize + cellSize / 2f;

        // Return the snapped position with the same Y position as the original object
        return new Vector3(snappedX, transform.position.y, snappedZ);
    }

    private void ApplyDamage(int dmg)
    {
        HP -= dmg;
    }

    private void OnDestroy()
    {
        // Unregister defense transform from the game manager
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