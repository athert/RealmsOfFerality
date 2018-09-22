using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathX : MonoBehaviour
{
    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
