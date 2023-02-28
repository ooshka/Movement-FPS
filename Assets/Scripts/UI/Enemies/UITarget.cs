using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITarget : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    [SerializeField]
    private float offset;

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + new Vector3(0, offset);
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
    }
}
