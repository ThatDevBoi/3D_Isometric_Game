using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        FieldOfView fow = (FieldOfView)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fow.transform.position, Vector3.up, Vector3.forward, 360, fow.viewRadius);
        //Vector3 viewAngleA = fow.DirectionFromAngle(-fow.viewAngle / 2, false);
        //Vector3 viewAngleB = fow.DirectionFromAngle(fow.viewAngle / 2, false);

        //Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleA * fow.viewRadius);
        //Handles.DrawLine(fow.transform.position, fow.transform.position + viewAngleB * fow.viewRadius);

        Handles.color = Color.red;
        foreach (Transform visabletarget in fow.visableTargets) 
        {
            Handles.DrawLine(fow.transform.position, visabletarget.position);
        }
        if(fow.UnitBeingAttacked !=null)
        {
            Handles.color = Color.blue;
            Handles.DrawLine(fow.transform.position, fow.UnitBeingAttacked.transform.position);
        }

    }
}
