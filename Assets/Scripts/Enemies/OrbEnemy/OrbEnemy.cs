using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbEnemy : Enemy
{
    [SerializeField]
    private float changeAngleTime = 2f;
    [SerializeField]
    private float rotationSpeed = 1f;
    private float minVerticalAngle = 15f;
    private float maxVerticalAngle = 50f;

    private float minHorizontalAngle = 0f;
    private float maxHorizontalAngle = 360f;

    private float currentVerticalAngle, currentHorizontalAngle;
    private float targetVerticalAngle, targetHorizontalAngle;

    private bool patrolling = false;

    private new Renderer renderer;

    protected override void Start()
    {
        base.Start();

        renderer = GetComponent<Renderer>();
    }

    protected override void Update()
    {
        base.Update();

        // Smoothly rotate towards the target vertical angle
        currentVerticalAngle = Mathf.Lerp(currentVerticalAngle, targetVerticalAngle, Time.deltaTime * rotationSpeed);

        // Smoothly rotate towards the target horizontal angle
        currentHorizontalAngle = Mathf.Lerp(currentHorizontalAngle, targetHorizontalAngle, Time.deltaTime * rotationSpeed);

        // Apply the rotation
        transform.eulerAngles = new Vector3(currentVerticalAngle, currentHorizontalAngle, transform.eulerAngles.z);

        Debug.DrawRay(transform.position, transform.forward, Color.cyan);
    }


    protected override void Patrol()
    {
        if (!patrolling)
        {
            patrolling = true;
            Debug.Log("Start patrolling");
            StartCoroutine(ChangeTargetAngle());
            renderer.material.color = Color.blue;
        }
    }

    IEnumerator ChangeTargetAngle()
    {
        while (patrolling)
        {
            targetVerticalAngle = Random.Range(minVerticalAngle, maxVerticalAngle);
            targetHorizontalAngle = Random.Range(minHorizontalAngle, maxHorizontalAngle);
            yield return new WaitForSeconds(changeAngleTime);
        }
    }

    protected override void Attack()
    {
        renderer.material.color = Color.red;
        patrolling = false;
    }
}
