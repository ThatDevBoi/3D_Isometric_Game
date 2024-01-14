using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public abstract class AI_Attack : ScriptableObject
{
    public abstract void ExecuteAttack(Transform attacker, Transform target);
}

[CreateAssetMenu(fileName = "NewScriptableObject", menuName = "AI/Attack/Melee Attack")]
public class MeleeAttack : AI_Attack
{
    public GameObject Arms;
    public AIStates States;
    public override void ExecuteAttack(Transform attacker, Transform target)
    {
        AIDamage dmg = Arms.GetComponentInChildren<AIDamage>();
        dmg.DMG = (int)States.DMG;
    }
}

[CreateAssetMenu(fileName = "NewScriptableObject", menuName = "AI/Attack/Ranged Attack")]
public class RangedAttack : AI_Attack
{
    public GameObject projectile;
    public float projectileSpeed;
    public AIStates state;
    
    public override void ExecuteAttack(Transform attacker, Transform target)
    {
        GameObject pro = Instantiate(projectile, attacker.position, Quaternion.identity) as GameObject;

        AIDamage DMGStates = pro.GetComponent<AIDamage>();
        DMGStates.DMG = (int)state.DMG;

        Vector3 dist = (target.transform.position - attacker.transform.position).normalized;
        Rigidbody pro_rb = pro.GetComponent<Rigidbody>();
        if (pro_rb != null)
        {
            pro_rb.velocity = dist * projectileSpeed;
        }
    }
}
