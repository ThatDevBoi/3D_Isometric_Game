using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class VillageItem : MonoBehaviour
{
    public Stats Stats;
    public GM gameManager;
    public CinemachineVirtualCamera pivotCamera;
    [SerializeField]
    Transform pivotRotateEmpty;

    [SerializeField]
    Vector3 startPosition;
    public Vector3 endposition;
    public float smoothSpeed = 3f;
    public float rotSpeed = 10f;
    bool isMoving = false;
    Transform parentHolder;
    float step;
    [SerializeField]
    bool startRotate;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.name = Stats.Name;

        Stats.HP -= 50;
        //Debug.Log(Stats.HP);
        // get the home position of each piece of the village 
        //startPosition = transform.localPosition;
        endposition = new Vector3(transform.position.x, transform.position.y + 10, transform.position.z);
        startPosition = transform.position;

        parentHolder = gameObject.transform.parent;

        pivotRotateEmpty = gameObject.transform.GetChild(0).GetComponent<Transform>();

        //GetComponent<Rigidbody>().isKinematic = true;



    }

    // Update is called once per frame
    void Update()
    {
        if(isMoving)
            StartCoroutine(MovementUp());

        if (startRotate)
        {
            // rotate the cam 

            pivotRotateEmpty.Rotate(Vector3.up * Time.deltaTime * rotSpeed);
        }
        else
        {
            // reset values 

            // reset camera values here 
            // rotation value is the same as the values we'll record 

        }


    }

    public void StartMoving()
    {
        isMoving = true;
        parentHolder = gameObject.transform.parent;
        transform.SetParent(null, true);
    }

    public IEnumerator MovementUp()
    {
        while (isMoving)
        {
            step = smoothSpeed * Time.deltaTime;
            //Debug.Log(step);
            float distanceToTarget = Vector3.Distance(transform.position, endposition);


            //Debug.Log(distanceToTarget);

            if (distanceToTarget <= step)
            {
                //Debug.Log(distanceToTarget);
                //Debug.Log(step);
                transform.position = endposition; // Set position directly if very close
                isMoving = false;
                transform.SetParent(parentHolder, true); // Reattach to the original parent

                startRotate = true;

            }
            else
            {
                Debug.Log(endposition);
                transform.position = Vector3.MoveTowards(transform.position, endposition, step);
                //Debug.Log(distanceToTarget);
                //Debug.Log(step);
            }

            yield return null;
        }

    }

    public IEnumerator MovementDown()
    {
        while (!isMoving)
        {
            step = smoothSpeed * Time.deltaTime * 8;
            //Debug.Log(step);
            float distanceToTarget = Vector3.Distance(transform.position, startPosition);

            //Debug.Log(distanceToTarget);

            if (distanceToTarget <= step)
            {
                //Debug.Log(distanceToTarget);
                //Debug.Log(step);
                transform.position = startPosition; // Set position directly if very close
                isMoving = false;
                transform.SetParent(parentHolder, true); // Reattach to the original parent


            }
            else
            {
                Debug.Log(endposition);
                transform.position = Vector3.MoveTowards(transform.position, startPosition, step);
                //Debug.Log(distanceToTarget);
                //Debug.Log(step);

                startRotate = false;

            }

            yield return null;
        }
    }

}

[CreateAssetMenu]
public class Stats : ScriptableObject
{
    public string Name = "";
    public float HP;
    public bool defenseItem = false;
    public float outputDMG;
}
