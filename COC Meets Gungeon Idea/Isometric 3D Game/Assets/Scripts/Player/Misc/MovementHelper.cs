using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Main.Player_Locomotion
{
    public static class MovementHelper
    {
        private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));

        public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
    }
}

