using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{

    private PlayerMotor motor;

    [HideInInspector]
    public enum State
    {
        WALKING, SPRINTING, IDLE, CROUCH_IDLE, CROUCH_WALKING, SLIDING, AIRBORNE, MELEE, JUMPING
    }

    // Start is called before the first frame update
    void Start()
    {
        motor = GetComponent<PlayerMotor>();
    }

    // Update is called once per frame
    void Update()
    {
        List<State> states = motor.GetState();
        string stateString = "";
        foreach (State state in states)
        {
            stateString += " " + state.ToString();
        }
        Debug.Log(stateString);
    }
}
