using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{

    public Camera cam;
    private float xRotation = 0f;
    public float sensitivity = 30f;
    
    public void ProcessLook(Vector2 input)
    {
        float mouseX = input.x;
        float mouseY = input.y;

        VerticalLook(mouseY * Time.deltaTime * sensitivity);

        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * sensitivity);
    }

    public void VerticalLook(float angle)
    {
        xRotation -= angle;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
    }
}
