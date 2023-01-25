using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallColliderScript : MonoBehaviour
{
    private bool colliding;

    public bool IsColliding()
    {
        return colliding;
    }

    private void OnTriggerEnter(Collider other)
    {
        colliding = true;
    }

    private void OnTriggerExit(Collider other)
    {
        colliding = false;
    }
}
