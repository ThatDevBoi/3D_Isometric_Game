using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]   // adds Rigibody
public class AI_Unit : MonoBehaviour
{
    public AIStates state;
    

    public Transform nearestTarget;

    public int LayerOfInterest;
    float distance = Mathf.Infinity;
    float dist = Mathf.Infinity;

    Rigidbody physics;

    float HP;

    public AI_Attack AttackType;
    float lastFireTime;

    // Start is called before the first frame update
    void Start()
    {
        GM.Instance.RegisterAIUnit(this);

        gameObject.name = state.name;   // set name
        HP = state.HP;


        physics = GetComponent<Rigidbody>();
        //NearestDefence();

    }

    // Update is called once per frame
    void Update()
    {
        MoveTowardsDefence();   // Move AI each frame

        // When I have no health I need to die
        if(HP <= 0)
        {
            Destroy(gameObject);
        }

        nearestTarget = GM.Instance.GetNearestDefense(transform.position);

        //// Check for destroyed Transforms and remove them from the list
        //for (int i = AllDefences.Count - 1; i >= 0; i--)
        //{
        //    if (AllDefences[i] == null)
        //    {
        //        AllDefences.RemoveAt(i);
        //    }
        //}

        //// Get the nearest defense Transform
        //nearestTarget = NearestDefence();
    }
    // Obtain the nearest enemy in a List of Objects
    //Transform NearestDefence()
    //{
    //    if (nearestTarget == null || !AllDefences.Contains(nearestTarget.gameObject))
    //    {
    //        nearestTarget = null; // Reset nearestTarget to null if it's no longer in the list

    //        for (int i = 0; i < AllDefences.Count; i++)
    //        {
    //            if (AllDefences[i] != null)
    //            {
    //                dist = Vector3.Distance(gameObject.transform.position, AllDefences[i].transform.position);
    //            }

    //            if (dist < distance)
    //            {
    //                nearestTarget = AllDefences[i].transform;
    //                distance = dist;
    //            }
    //        }
    //    }

    //    return nearestTarget; ;
    //}

    void MoveTowardsDefence()
    {
        // if I am not within distance of the Defence I am running towards and I've not gotten close enough within attacking Range
        if (nearestTarget != null && Vector3.Distance(transform.position, nearestTarget.position) > state.rangeUntilAttack)
        {
            // Make sure I can find the nearest enemy First
            Vector3 Direction = (nearestTarget.position - transform.position).normalized;
            physics.velocity = Direction * state.moveSpeed; // Move towards the enemy
        }
        else
        {
            if (nearestTarget != null)
            {
                if (Time.time - lastFireTime >= state.timeBetweenAttacks)
                {
                    AttackType.ExecuteAttack(this.transform, nearestTarget);
                    lastFireTime = Time.time;
                }
            }

        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "Projectile")
        {
            // Apply Damage and pass through Value 
            ApplyDamage(col.GetComponent<DefenceProjectile>().DMG);
            Destroy(col.gameObject);    // Destroy Projectile
            
        }
    }
    // Apply Damage if hit - call where needed
    public void ApplyDamage(int damage)
    {
        HP -= damage;
    }

    private void OnDestroy()
    {
        GM.Instance.UnregisterAIUnit(this);
    }
}

[CreateAssetMenu(fileName = "NewScriptableObject", menuName = "AI/Stats/Base Stats")]
public class AIStates : ScriptableObject
{
    [Tooltip("Name of the AI Unit")]
    public string name;
    [Tooltip("Max amount of health the unit will start with")]
    public float HP;
    [Tooltip("Damage Output the Unit does to Targets")]
    public float DMG;
    // Weak against Enum?
    [Tooltip("How much space does tis AI take in the Army?")]
    public float houseSpace;
    [Tooltip("Range until the AI stops and starts attacking")]
    public int rangeUntilAttack;
    [Tooltip("Speed When moving towards a Defence / Village Piece")]
    public int moveSpeed;
    [Tooltip("Time Taken To Attack Again")]
    public float timeBetweenAttacks;

}
