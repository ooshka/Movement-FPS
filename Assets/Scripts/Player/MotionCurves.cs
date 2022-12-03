using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionCurves : MonoBehaviour
{
   public static float LinearInterp(float currentY, float y0, float y1, float totalTime)
    {
        
        // in case the value we are trying to interpolate is outside of the given range
        if (y1 > y0)
        {
            currentY = Mathf.Clamp(currentY, y0, y1);
        } else
        {
            currentY = Mathf.Clamp(currentY, y1, y0);
        }

        // get the fake elapsed time by seeing what percentage of the interpolation we've gotten to
        float elapsedTime = (currentY - y0) / (y1 - y0) * totalTime;
        float newTime = elapsedTime + Time.deltaTime;

        // calculate new value based on time since last physics calc
        float newY = Mathf.Min(newTime/totalTime, 1) * (y1 - y0) + y0;
        return newY;
    }
}
