using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AI_Attack : ScriptableObject
{
    public abstract void ExecuteAttack(Transform attacker, Transform target);
}

[CreateAssetMenu(fileName = "NewScriptableObject", menuName = "AI/Attack/Melee Attack")]
public class MeleeAttack : AI_Attack
{
    public override void ExecuteAttack(Transform attacker, Transform target)
    {
        Debug.Log("Attacking");
        // In future animations will show attacking
    }
}

[CreateAssetMenu(fileName = "NewScriptableObject", menuName = "AI/Attack/Ranged Attack")]
public class RangedAttack : AI_Attack
{
    public GameObject projectile;
    public float projectileSpeed;
    
    public override void ExecuteAttack(Transform attacker, Transform target)
    {
        GameObject pro = Instantiate(projectile, attacker.position, Quaternion.identity) as GameObject;

        Vector3 dist = (target.transform.position - attacker.transform.position).normalized;
        Rigidbody pro_rb = pro.GetComponent<Rigidbody>();
        if (pro_rb != null)
        {
            pro_rb.velocity = dist * projectileSpeed;
        }
    }
}
