using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    private Crosshair crosshair;

    // Start is called before the first frame update
    void Start()
    {
        crosshair = GetComponent<Crosshair>();
        crosshair.generateCrosshair(gameObject);
    }
}
