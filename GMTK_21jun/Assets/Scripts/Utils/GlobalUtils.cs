using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalUtils
{
    public static Vector3 GetXY(this Vector3 v, float z = 0f)
    {
        return new Vector3(v.x, v.y, z);
    }

    public static Vector3 GetSwarmOffset(this int index)
    {
        int level = 1;
        int temp = 6;
        while (level < 10)
        {
            if (index <= temp)
            {
                temp = index - Mathf.RoundToInt(Mathf.Pow(2, level - 1) - 1) * 6;
                break;
            }

            level += 1;
            temp += temp * 2;
        }

        int round = Mathf.RoundToInt(Mathf.Pow(2, level - 1)) * 6;
        float angle = (2 * Mathf.PI / round) * temp;
        
        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle) * 0.7f) * level;
    }
}
