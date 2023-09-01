using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{

    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;

    public GameObject UnitBeingAttacked;
    [HideInInspector]
    public bool attackingUnite;

    public LayerMask targetmask;
    public LayerMask ObstacleMask;

    public List<Transform> visableTargets = new List<Transform>();

    private void Start()
    {
        StartCoroutine("FindTargetWithDelay", .2f);
    }

    IEnumerator FindTargetWithDelay (float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds (delay);
            FindVisableTargets();
        }
    }

    void FindVisableTargets()
    {
        visableTargets.Clear ();    // remove from array 
        // build the array with data in the radius 
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetmask);
        // Make a loop for each array part 
        for(int i = 0; i < targetsInViewRadius.Length; i++)
        {
            // Fill new Tranform Component and add the the position in Array
            Transform Target = targetsInViewRadius[i].transform;
            // Direction to the target from the current position 
            Vector3 dirToTarget = (Target.position - transform.position).normalized;
            // 

            // 

            // Distance between me and the object I'm looking at 
            float dstToTarget = Vector3.Distance(transform.position, Target.position);
            // Detect and see if a object is being blocked by the obstacle mask layer 
            if (Physics.Raycast(transform.position, dirToTarget, dstToTarget, ObstacleMask))
                visableTargets.Remove(Target);  // Remove that object 
            else
                visableTargets.Add(Target); // if not add the object to the list 

        }


        foreach (Transform Target in visableTargets)
        {
            float dst = Vector3.Distance (transform.position, Target.position);
            if (dst < viewRadius && !attackingUnite)
            {
                UnitBeingAttacked = Target.gameObject;
                attackingUnite = true;
            }

        }

    }


    public Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
