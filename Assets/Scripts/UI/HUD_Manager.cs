using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD_Manager : MonoBehaviour
{

    public Text velocityText;
    private PlayerMotor playerMotor;
    // Start is called before the first frame update
    void Start()
    {
        playerMotor = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMotor>();
    }
    // Update is called once per frame
    void Update()
    {
        velocityText.text = "Velocity: " + playerMotor._playerVelocity.magnitude;
    }
}
