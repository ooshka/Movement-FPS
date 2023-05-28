using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        GetComponentInChildren<Interactable>().Subscribe(OnPickup);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnPickup()
    {
        Debug.Log("Weapon Picked Up");
        Destroy(gameObject);
    }
}
