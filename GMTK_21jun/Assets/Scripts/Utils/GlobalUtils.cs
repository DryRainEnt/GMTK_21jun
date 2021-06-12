using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalUtils
{
    public static Vector3 GetXY(this Vector3 v, float z = 0f)
    {
        return new Vector3(v.x, v.y, z);
    }
}
