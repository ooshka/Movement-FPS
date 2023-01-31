using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour
{
    private Animator anim;

    [HideInInspector]
    public enum State
    {
        WALKING, SPRINTING, IDLE, CROUCH_IDLE, CROUCH_WALKING, SLIDING, AIRBORNE, MELEE, JUMPING, CLIMBING, VAULTING, SHOOTING, RELOADING
    }

    private readonly string MELEE_TRIGGER = "Melee_Trigger";
    private readonly string JUMPING_TRIGGER = "Jump_Trigger";
    private readonly string MOVEMENT_BLEND = "Movement_Blend";

    private readonly float blendTime = 0.5f;

    private readonly List<State> playerState = new List<State>();
    private readonly List<State> gunState = new List<State>();
    private readonly List<State> totalState = new List<State>();

    // Start is called before the first frame update
    void Start()
    {
        anim = GameObject.Find("Player_Arms").GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        totalState.Clear();
        totalState.AddRange(playerState);
        totalState.AddRange(gunState);
        
        if (totalState.Contains(State.MELEE))
        {
            anim.SetTrigger(MELEE_TRIGGER);
            playerState.Remove(State.MELEE);
        }
        else if (totalState.Contains(State.JUMPING))
        {
            anim.SetTrigger(JUMPING_TRIGGER);
            playerState.Remove(State.JUMPING);
        }
        else if (totalState.Contains(State.SPRINTING))
        {
            anim.SetFloat(MOVEMENT_BLEND, 1f, blendTime, Time.deltaTime);
        }
        else if (totalState.Contains(State.IDLE))
        {
            anim.SetFloat(MOVEMENT_BLEND, 0f, blendTime, Time.deltaTime);
        }
        else if (totalState.Contains(State.WALKING))
        {
            anim.SetFloat(MOVEMENT_BLEND, 0.5f, blendTime, Time.deltaTime);
        }
    }

    public void SetPlayerState(List<State> playerState)
    {
        this.playerState.Clear();
        this.playerState.AddRange(playerState);
    }

    public void SetGunState(List<State> gunState)
    {
        this.gunState.Clear();
        this.gunState.AddRange(gunState);
    }
}
