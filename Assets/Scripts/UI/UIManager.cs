using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    private Crosshair crosshair;
    private Healthbar playerHealthBar;
    private Timer damageTimer;
    // Start is called before the first frame update
    void Start()
    {
        crosshair = GetComponent<Crosshair>();
        playerHealthBar = GetComponentInChildren<Healthbar>();
        crosshair.generateCrosshair(gameObject);

        damageTimer = new Timer(2f, false);
    }

    private void Update()
    {
        if (damageTimer.CanTriggerEventAndReset())
        {
            playerHealthBar.ApplyDamage(20f);
        }
        damageTimer.Iterate(Time.deltaTime);
    }

    private void DamageHealthBar()
    {
        playerHealthBar.ApplyDamage(20f);
    }
}
