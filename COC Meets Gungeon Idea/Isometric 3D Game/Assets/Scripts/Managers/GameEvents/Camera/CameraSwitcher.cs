using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Runtime.CompilerServices;

public class CameraSwitcher
{
    static List<CinemachineVirtualCamera> cameras = new List<CinemachineVirtualCamera>();

    public static CinemachineVirtualCamera activeCamera = null;

    public static bool IsCameraActive(CinemachineVirtualCamera cam)
    {
        return cam == activeCamera;
    }

    public static void SwitchCamera(CinemachineVirtualCamera cam)
    {
        cam.Priority = 10;
        activeCamera = cam;

        foreach (CinemachineVirtualCamera c in cameras)
        {
            if(c!=cam && cam.Priority != 0)
            {
                c.Priority = 0;
            }
        }
    }

    public static void Register(CinemachineVirtualCamera cam)
    {
        cameras.Add(cam);
       //Debug.Log("Camera Added" +  cam.name);
    }

    public static void UnRegister(CinemachineVirtualCamera cam)
    {
        cameras.Remove(cam);
        //Debug.Log("Camera Removed" + cam.name);
    }
}
