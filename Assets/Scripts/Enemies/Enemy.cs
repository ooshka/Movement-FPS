using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField]
    private EnemyUI uiPrefab;

    private EnemyUI uiInstance;
    private Canvas canvas;

    private Healthbar healthbar;

    private bool isInChaseRange = false;
    private bool isInAttackRange = false;

    private int playerLayerMask;

    [SerializeField]
    protected EnemyData data;
    protected Transform playerTransform;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        Debug.Log("Data: " + data);

        // get the "Player" layer
        int layer = LayerMask.NameToLayer("Player");
        playerLayerMask = 1 << layer;

        canvas = GameObject.Find("WorldSpaceCanvas").GetComponent<Canvas>();
        playerTransform = GameObject.Find("Player").transform;

        uiInstance = Instantiate(uiPrefab, canvas.transform);
        uiInstance.SetTarget(transform);
        healthbar = uiInstance.GetComponentInChildren<Healthbar>();
        healthbar.onDeath += OnDeath;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        isInAttackRange = Physics.CheckSphere(transform.position, data.attackSphereRadius, playerLayerMask);
        if (isInAttackRange)
        {
            Attack();
        } else
        {
            isInChaseRange = Physics.CheckSphere(transform.position, data.detectionSphereRadius, playerLayerMask);
            if (isInChaseRange)
            {
                Chase();
            } else
            {
                Patrol();
            }
        }
    }

    protected virtual void Attack()
    {
        // TODO: make abstract
    }

    protected virtual void Chase()
    {
        // TODO: make abstract and make sure we handle stationary enemies
    }

    protected virtual void Patrol()
    {
        // TODO: make abstract and make sure we handle stationary enemies
    }

    public void Damage(float amount)
    {
        healthbar.ApplyDamage(amount);
    }

    private void OnDeath()
    {
        Destroy(gameObject);
        Destroy(uiInstance.gameObject);
    }
}
