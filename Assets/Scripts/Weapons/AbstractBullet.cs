using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractBullet : MonoBehaviour
{
    private float damage;

    public GameObject impactObject;

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 hitLocation = collision.GetContact(0).point;

        GameObject impact = Instantiate(impactObject, hitLocation, Quaternion.identity);

        Destroy(impact, 3f);
        Destroy(gameObject);
    }
}
