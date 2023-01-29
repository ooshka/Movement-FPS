using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCollisionCheck : MonoBehaviour
{

    private readonly List<float> positions = new List<float>();
    private CharacterController controller;
    private readonly int numOfCheckPositions = 6;
    private readonly float rayDistance = 0.45f;
    public bool debug = true;

    // how's this for hacky? store out last RaycastHit in this so we can access the normal for wall jumping...
    // TODO: do something less hacky
    RaycastHit lastHit;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();

        float extent = transform.localScale.y;
        float spacing = extent * 2 / (numOfCheckPositions - 1);

        for (float i = -extent; i <= extent * 1.01f; i += spacing)
        {
            positions.Add(i);
        }
       
    }

    // Update is called once per frame
    void Update()
    {
        CanVault();
    }

    private bool CheckPosition(float verticalPosition)
    {
        Vector3 start = transform.localPosition + new Vector3(0, verticalPosition, 0);
        if (debug)
        {
            Debug.DrawLine(start, start + transform.forward * rayDistance, Color.blue, 0.2f);
        }
        return Physics.Raycast(start, transform.forward, out lastHit, rayDistance, 1 << LayerMask.NameToLayer("Terrain"));
    }

    private bool CheckPositions(int start, int end)
    {
        bool isColliding = true;
        for (int i = start; i <= end; i++)
        {
            isColliding &= CheckPosition(positions[i]);
        }
        return isColliding;
    }

    public bool CanWallJump()
    {
        return CheckPositions(1, 3);
    }

    public bool CanClimb()
    {
        return CheckPositions(2, 4);
    }

    public bool CanVault()
    {
        return CheckPositions(2, 4) && !CheckPosition(positions[5]);
    }

    public Vector3 getLastHitNormal()
    {
        return lastHit.normal;
    }
}
