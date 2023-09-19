using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    private Crosshair crosshair;
    private Healthbar playerHealthBar;
    // Start is called before the first frame update
    void Start()
    {
        crosshair = GetComponent<Crosshair>();
        playerHealthBar = GetComponentInChildren<Healthbar>();
        crosshair.generateCrosshair(gameObject);
    }

    private void Update()
    {

    }

}
