using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallMechanics : MonoBehaviour
{
    public GameObject wallCollider;

    private CharacterController controller;
    private float colliderDepth;
    // found through trial and error
    private readonly float forwardDistance = 0.575f;
    private int numOfColliders = 6;

    private readonly List<WallColliderScript> colliders = new List<WallColliderScript>();

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();

        float totalHeight = controller.height;

        float spacing = totalHeight / (numOfColliders - 1);

        for (float i = - totalHeight / 2; i <= totalHeight / 2; i += spacing)
        {
            GameObject collider = Instantiate(wallCollider, transform);
            collider.transform.localPosition = new Vector3(0, i, forwardDistance);
            colliders.Add(collider.GetComponent<WallColliderScript>());
        }
    }

    private void Update()
    {
        Debug.Log(CanWallJump());
    }

    public bool CanWallJump()
    {
        return AreCollidersInContact(1, 3);
    }
    
    public bool CanClimb()
    {
        return AreCollidersInContact(1, 3);
    }

    public bool CanVault()
    {
        return AreCollidersInContact(3, 4) && !AreCollidersInContact(5, 5);
    }


    private bool AreCollidersInContact(int startIndex, int endIndex)
    {
        bool isColliding = true;
        for (int i = startIndex; i <= endIndex; i++)
        {
            isColliding &= colliders[i].IsColliding();
        }
        return isColliding;
    }
}
