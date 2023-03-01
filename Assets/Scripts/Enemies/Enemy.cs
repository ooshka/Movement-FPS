using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField]
    private EnemyUI uiPrefab;
    private EnemyUI uiInstance;
    [SerializeField]
    private Canvas canvas;

    private Healthbar healthbar;

    // Start is called before the first frame update
    void Start()
    {
        uiInstance = Instantiate(uiPrefab, canvas.transform);
        uiInstance.SetTarget(transform);
        healthbar = uiInstance.GetComponentInChildren<Healthbar>();
        healthbar.onDeath += OnDeath;
    }

    // Update is called once per frame
    void Update()
    {
        
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
