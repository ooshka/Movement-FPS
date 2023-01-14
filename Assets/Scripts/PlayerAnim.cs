using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour
{

    private PlayerMotor motor;
    private Animator anim;

    [HideInInspector]
    public enum State
    {
        WALKING, SPRINTING, IDLE, CROUCH_IDLE, CROUCH_WALKING, SLIDING, AIRBORNE, MELEE, JUMPING
    }

    private readonly string MELEE_TRIGGER = "Melee_Trigger";
    private readonly string JUMPING_TRIGGER = "Jump_Trigger";
    private readonly string MOVEMENT_BLEND = "Movement_Blend";

    private readonly float blendTime = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        anim = GameObject.Find("Player_Arms").GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        List<State> states = motor.GetState();
        
        if (states.Contains(State.MELEE))
        {
            anim.SetTrigger(MELEE_TRIGGER);
        } else if (states.Contains(State.JUMPING))
        {
            anim.SetTrigger(JUMPING_TRIGGER);
        } else if (states.Contains(State.SPRINTING))
        {
            anim.SetFloat(MOVEMENT_BLEND, 1f, blendTime, Time.deltaTime);
        } else if (states.Contains(State.IDLE))
        {
            anim.SetFloat(MOVEMENT_BLEND, 0f, blendTime, Time.deltaTime);
        } else if (states.Contains(State.WALKING)) {
            anim.SetFloat(MOVEMENT_BLEND, 0.5f, blendTime, Time.deltaTime);
        }
    }
}
