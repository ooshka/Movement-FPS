using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    private float angleCutoff = 30f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            Camera cam = collider.gameObject.GetComponentInChildren<Camera>();

            Vector3 camDirection = cam.transform.forward;

            Vector3 itemDirection = transform.position - cam.transform.position;
            itemDirection.Normalize();

            float lookAngle = Vector3.Angle(camDirection, itemDirection);

            if (lookAngle < angleCutoff)
            {
                Debug.Log("Looking at the thing!!!");
            }
        }
    }
}
