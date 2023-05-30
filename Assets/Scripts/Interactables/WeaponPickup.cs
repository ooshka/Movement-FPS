using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField]
    private AbstractGun prefabGun;

    // Start is called before the first frame update
    void Start()
    {
        GetComponentInChildren<Interactable>().Subscribe(OnPickup);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnPickup(Collider playerCollider)
    {
        AbstractGun gun = Instantiate(prefabGun, prefabGun.transform.position, prefabGun.transform.rotation);
        playerCollider.GetComponent<PlayerManager>().PickupGun(gun);
        Destroy(gameObject);
    }
}
