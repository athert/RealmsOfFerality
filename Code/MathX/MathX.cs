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
    public static bool ParseIntArrayFromString(string text, out int[] valueArray)
    {
        List<int> valueList = new List<int>();
        string[] parsedText = text.Split(' ');

        for (int i = 0; i < parsedText.Length; i++)
        {
            if (parsedText[i] == null || parsedText[i].Replace(" ", "") == "")
                continue;

            int value = 0;

            if (!int.TryParse(parsedText[i], out value))
            {
                valueArray = new int[0];
                return false;
            }
            valueList.Add(value);
        }
        valueArray = valueList.ToArray();
        return true;
    }
    public static bool ParseFloatArrayFromString(string text, out float[] valueArray)
    {
        if (text == null || text == "")
        {
            valueArray = new float[0];
            return false;
        }

        List<float> valueList = new List<float>();
        string[] parsedText = text.Split(' ');

        for (int i = 0; i < parsedText.Length; i++)
        {
            if (parsedText[i] == null || parsedText[i].Replace(" ", "") == "")
                continue;

            float value = 0;

            if (!float.TryParse(parsedText[i], out value))
            {
                valueArray = new float[0];
                return false;
            }
            valueList.Add(value);
        }
        valueArray = valueList.ToArray();
        return true;
    }
}
