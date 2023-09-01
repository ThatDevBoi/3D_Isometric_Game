using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Unit : MonoBehaviour
{
    public AIStates state;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[CreateAssetMenu]
public class AIStates : ScriptableObject
{
    public string name;
    public float HP;
    float currentHP;

    public float DMG;
    float currentDMG;




}
