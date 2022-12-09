using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{

    private Slider primaryHealth, secondaryHealth;

    public float _damageAmount = 20f;
    public float _maxHealth = 100f;
    public float _secondaryHealthSpeed = 0.5f;
    private float _currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        primaryHealth = GameObject.Find("PrimaryFillContainer").GetComponent<Slider>();
        secondaryHealth = GameObject.Find("SecondaryFillContainer").GetComponent<Slider>();

        _currentHealth = _maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (primaryHealth.value < secondaryHealth.value)
        {
            UpdateSecondaryHealth();
        }
    }

    public void ApplyDamage(float damage)
    {
        _currentHealth -= damage;
        float newSliderValue = Mathf.Max(0, _currentHealth / _maxHealth);
        primaryHealth.value = newSliderValue;
    }

    void UpdateSecondaryHealth()
    {
        secondaryHealth.value -= _secondaryHealthSpeed * Time.deltaTime;
    }

}
