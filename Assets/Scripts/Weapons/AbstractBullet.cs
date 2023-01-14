using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractBullet : MonoBehaviour
{
    public GameObject impactObject;
    public GunData data;

    private void Start()
    {
        // ignore collisions with player
        Physics.IgnoreCollision(GetComponent<Collider>(), data.playerCollider);

        float ttl = data.maxDistance / data.bulletSpeed;
        Destroy(gameObject, ttl);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 hitLocation = collision.GetContact(0).point;

        GameObject impact = Instantiate(impactObject, hitLocation, Quaternion.identity);

        Destroy(impact, 3f);
        Destroy(gameObject);
    }
}
