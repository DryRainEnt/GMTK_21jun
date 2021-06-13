using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollectType
{
    Player,
    Virtual,
}
public static class GlobalUtils
{
    public static Vector3 GetXY(this Vector3 v, float z = 0f)
    {
        return new Vector3(v.x, v.y, z);
    }

    public static Vector3 GetSwarmOffset(this int index, CollectType type = CollectType.Player, float distMult = 1f)
    {
        switch (type)
        {
            default:
            case CollectType.Player:
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
        
                return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle) * 0.7f) * (level * distMult);
            
            case CollectType.Virtual:
                float stock = (2f * Mathf.PI) / 3f;
                for (int i = 0; i < index; i++)
                {
                    stock += (2f * Mathf.PI) / (3f + i * 0.5f);
                }

                return new Vector3(Mathf.Cos(stock), Mathf.Sin(stock) * 0.7f) * (0.2f + index * 0.05f);
        }
    }

    public static int GetSwarmLevel(this int count)
    {
        int level = 1;
        int temp = 6;
        while (level < 10)
        {
            if (count <= temp) break;
            level += 1;
            temp += temp * 2;
        }

        return level;
    }

    public static Vector3 RandomWholeRange(float distance = 1f)
    {
        float x = (Random.value - 0.5f) * 2f;
        float y = (Random.value - 0.5f) * 1.4f;

        var ret = new Vector3(x, y).normalized;
        return ret * (Random.value * distance);
    }
    
    public static Vector3 RandomRange(float distance = 1f)
    {
        float x = (Random.value);
        float y = (Random.value) * 0.7f;

        var ret = new Vector3(x, y).normalized;
        return ret * (Random.value * distance);
    }
    
    public static Vector2 ToVector2(this Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }
    
    public static Vector3 ToVector3(this Vector2 v, float z = 0f)
    {
        return new Vector3(v.x, v.y, z);
    }

    public static float GetAngle(this Vector2 vector)
    {
        var angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        return angle;
    }

    public static Vector3 Coefficient(this Vector3 v, Vector3 co)
    {
        return new Vector3(v.x * co.x, v.y * co.y, v.z * co.z);
    } 
    
    public static Vector2 Coefficient(this Vector2 v, Vector2 co)
    {
        return new Vector3(v.x * co.x, v.y * co.y);
    }
}
