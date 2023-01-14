using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom UI/Crosshair")]
public class CrosshairData : ScriptableObject
{
    // TODO: drive all of this via settings
    public float length, width, borderThickness, spacing;
    public Color color;
}
