using UnityEngine;

public static class Utility
{
    public static float Remap(float v, float iMin, float iMax, float oMin, float oMax)
    {
        return Mathf.Lerp(oMin, oMax, Mathf.InverseLerp(iMin, iMax, v));
    }
}