using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD_Manager : MonoBehaviour
{

    public TMP_Text velocityText;
    private PlayerMotor playerMotor;
    private Button damageButton;
    private HealthBar healthBar;

    public float _damageAmount = 20f;
    public float _maxHealth = 100f;
  
    // Start is called before the first frame update
    void Start()
    {
        healthBar = GetComponent<HealthBar>();
        playerMotor = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMotor>();
        damageButton = GameObject.Find("DamageButton").GetComponent<Button>();

        damageButton.onClick.AddListener(() => healthBar.ApplyDamage(_damageAmount));
    }
    // Update is called once per frame
    void Update()
    {
        velocityText.text = "Velocity: " + (int) playerMotor._playerVelocity.magnitude;
    }
}
