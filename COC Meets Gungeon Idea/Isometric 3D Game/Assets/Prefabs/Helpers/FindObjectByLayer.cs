using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindObjectByLayer : MonoBehaviour
{
    /// <summary>
    /// Finds Objects by Layer and adds to a List in another class
    /// </summary>
    /// <param name="layerIndex"></param>
    /// <param name="goList"></param>
    public static void FindAndSelectObjectsByLayer(int layerIndex, List<GameObject> goList)
    {
        goList.Clear();

        GameObject[] ObjectsByLayer = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in ObjectsByLayer)
        {
            if(obj.layer == layerIndex)
            {
                goList.Add(obj);
            }
        }
    }
}
