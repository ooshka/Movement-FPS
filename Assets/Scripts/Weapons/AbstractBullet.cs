using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractBullet : MonoBehaviour
{
    public GameObject impactObject;
    public BulletData data;

    private int ignoreLayer;
    private void Start()
    {
        // we need to ignore the collider of the shooter
        if (data.parent == BulletData.Parent.PLAYER)
        {
            ignoreLayer = LayerMask.NameToLayer("Player");
        } else
        {
            ignoreLayer = LayerMask.NameToLayer("Enemies");
        }

        float ttl = data.maxDistance / data.bulletSpeed;
        Destroy(gameObject, ttl);
    }

    public void Fire(Vector3 direction)
    {
        transform.forward = direction;
        GetComponent<Rigidbody>().AddForce(data.bulletSpeed * transform.forward, ForceMode.VelocityChange);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(LayerMask.LayerToName(collision.collider.gameObject.layer));

        if (collision.collider.gameObject.layer != ignoreLayer)
        {

            Vector3 hitLocation = collision.GetContact(0).point;

            if (collision.gameObject.TryGetComponent(out IDamageable hit))
            {
                hit.Damage(data.baseDamage);
            } else
            {
                GameObject impact = Instantiate(impactObject, hitLocation, Quaternion.identity);
                Destroy(impact, 3f);
            }
            Destroy(gameObject);
        }
    }
}
