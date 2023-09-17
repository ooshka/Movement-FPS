using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbEnemy : Enemy
{
    [SerializeField]
    Material laserMaterial;

    private float changeAngleTime = 2f;
    private float rotationSpeed = 1f;
    private float minVerticalAngle = 15f;
    private float maxVerticalAngle = 50f;

    private float minHorizontalAngle = 0f;
    private float maxHorizontalAngle = 360f;

    private float chargeAttackAngleThreshold = 15f;

    private float currentVerticalAngle, currentHorizontalAngle;
    private float targetVerticalAngle, targetHorizontalAngle;

    private bool isPatrolling = false;
    private bool isChargingAttack = false;

    int rayCastLayerMask;

    private new Renderer renderer;
    private LineRenderer lineRenderer;

    private Timer chargeCooldownTimer = new (2f, false);

    protected override void Start()
    {
        base.Start();

        rayCastLayerMask = Physics.AllLayers & ~(1 << LayerMask.NameToLayer("Enemies"));

        renderer = GetComponent<Renderer>();
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = laserMaterial;
        lineRenderer.widthMultiplier = 0.01f;
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
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

        if (isChargingAttack && lineRenderer.enabled)
        {
            if (!HasLineOfSight())
            {
                // if we've broken LOS cancel the charge and give a small cooldown
                lineRenderer.enabled = false;
                isChargingAttack = false;
                chargeCooldownTimer.SetTime(1f);
            }

            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, playerTransform.position);
        }

        chargeCooldownTimer.Iterate(Time.deltaTime);
    }


    protected override void Patrol()
    {
        if (!isPatrolling && !isChargingAttack)
        {
            rotationSpeed = 1f;
            isPatrolling = true;
            StartCoroutine(ChangeTargetAngle());
            renderer.material.color = Color.blue;
        }
    }

    IEnumerator ChangeTargetAngle()
    {
        while (isPatrolling)
        {
            targetVerticalAngle = Random.Range(minVerticalAngle, maxVerticalAngle);
            targetHorizontalAngle = Random.Range(minHorizontalAngle, maxHorizontalAngle);
            yield return new WaitForSeconds(changeAngleTime);
        }
    }

    protected override void Attack()
    {
        if (HasLineOfSight())
        {
            // only need to do this stuff on state change
            if (isPatrolling)
            {
                rotationSpeed = 8f;
                renderer.material.color = Color.red;
                isPatrolling = false;
            }

            // Get the direction vector from the enemy to the player
            Vector3 directionToPlayer = playerTransform.position - transform.position;

            // Calculate the horizontal and vertical angles
            float horizontalAngle = Mathf.Atan2(directionToPlayer.x, directionToPlayer.z) * Mathf.Rad2Deg;
            float verticalAngle = Mathf.Atan2(directionToPlayer.y, new Vector3(directionToPlayer.x, 0, directionToPlayer.z).magnitude) * Mathf.Rad2Deg;

            // Set the target angles
            targetHorizontalAngle = horizontalAngle;
            targetVerticalAngle = -verticalAngle; // The angle might need to be negated depending on your setup

            float targetAngle = Vector3.Angle(transform.forward, directionToPlayer.normalized);
            if (targetAngle <= chargeAttackAngleThreshold && !isChargingAttack && chargeCooldownTimer.CanTriggerEvent())
            {
                isChargingAttack = true;
                StartCoroutine(ChargeAttack());
            }
        }
    }

    IEnumerator ChargeAttack()
    {
        // technically in hertz
        float laserFrequency = 1.5f;
        while (laserFrequency <= 100 && isChargingAttack)
        {
            StartCoroutine(ShowLaser(1 / laserFrequency));
            yield return new WaitForSeconds(1 / laserFrequency);
            laserFrequency *= 1.5f;
        }
        if (isChargingAttack)
        {
            Debug.Log("Attack");
            isChargingAttack = false;
            chargeCooldownTimer.Reset();
        }
    }

    IEnumerator ShowLaser(float laserDuration)
    {
        lineRenderer.enabled = true;
        yield return new WaitForSeconds(laserDuration * 0.9f);

        lineRenderer.enabled = false;
    }

    private bool HasLineOfSight()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, playerTransform.position - transform.position, out hit, 200f, rayCastLayerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Terrain"))
            {
                return false;
            } else
            {
                return true;
            }
        } else
        {
            return false;
        }
    }

    private void Shoot()
    {

    }
}
