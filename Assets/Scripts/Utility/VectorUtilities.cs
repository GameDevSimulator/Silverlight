using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    static class VectorUtilities
    {
        public static Vector3 MultiplyComponents(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static Vector3 Pow(this Vector3 a, float b)
        {
            return new Vector3(Mathf.Pow(a.x, b), Mathf.Pow(a.y, b), Mathf.Pow(a.z, b));
        }
    }
}
