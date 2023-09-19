using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateController : MonoBehaviour
{
    private Animator animPlayer, animGun;

    [HideInInspector]
    public enum State
    {
        WALKING, SPRINTING, IDLE, CROUCH_IDLE, CROUCH_WALKING, SLIDING, AIRBORNE, MELEE, JUMPING, CLIMBING, VAULTING, VAULT_CANCEL, SHOOTING, RELOADING, ADS
    }

    private readonly string MELEE_TRIGGER = "Melee_Trigger";
    private readonly string JUMPING_TRIGGER = "Jump_Trigger";
    private readonly string VAULT_TRIGGER = "Vault_Trigger";
    private readonly string VAULT_CANCEL_TRIGGER = "Vault_Cancel_Trigger";
    private readonly string MOVEMENT_BLEND = "Movement_Blend";
    private readonly string SHOOT_TRIGGER = "Shoot_Trigger";
    private readonly string RELOAD_TRIGGER = "Reload_Trigger";
    private readonly string IS_CLIMBING = "Is_Climbing";
    private readonly string IS_ADS = "Is_ADS";
    private readonly string GUN_TYPE = "Gun_Type";


    private readonly float blendTime = 0.5f;

    private readonly List<State> playerState = new List<State>();
    private readonly List<State> gunState = new List<State>();
    private readonly List<State> totalState = new List<State>();


    // Start is called before the first frame update
    void Start()
    {
        animPlayer = GameObject.Find("PLAYER ARMS").GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        totalState.Clear();
        totalState.AddRange(playerState);
        totalState.AddRange(gunState);

        if (totalState.Contains(State.ADS))
        {
            animPlayer.SetBool(IS_ADS, true);
        }
        else
        {
            animPlayer.SetBool(IS_ADS, false);
        }

        if (totalState.Contains(State.SHOOTING))
        {
            animGun.SetTrigger(SHOOT_TRIGGER);
            animPlayer.SetTrigger(SHOOT_TRIGGER);
        } else if (totalState.Contains(State.RELOADING))
        {
            animGun.SetTrigger(RELOAD_TRIGGER);
            animPlayer.SetTrigger(RELOAD_TRIGGER);
        }

        if (totalState.Contains(State.CLIMBING))
        {
            animPlayer.SetBool(IS_CLIMBING, true);
        } else
        {
            animPlayer.SetBool(IS_CLIMBING, false);
        }

        if (totalState.Contains(State.VAULTING))
        {
            animPlayer.SetTrigger(VAULT_TRIGGER);
        } else
        {
            if (totalState.Contains(State.VAULT_CANCEL))
            {
                animPlayer.SetTrigger(VAULT_CANCEL_TRIGGER);
            }
            else if (totalState.Contains(State.MELEE))
            {
                animPlayer.SetTrigger(MELEE_TRIGGER);
                playerState.Remove(State.MELEE);
            }
            else if (totalState.Contains(State.JUMPING))
            {
                animPlayer.SetTrigger(JUMPING_TRIGGER);
                playerState.Remove(State.JUMPING);
            }
            else if (totalState.Contains(State.SPRINTING))
            {
                animPlayer.SetFloat(MOVEMENT_BLEND, 1f, blendTime, Time.deltaTime);
            }
            else if (totalState.Contains(State.IDLE))
            {
                animPlayer.SetFloat(MOVEMENT_BLEND, 0f, blendTime, Time.deltaTime);
            }
            else if (totalState.Contains(State.WALKING))
            {
                animPlayer.SetFloat(MOVEMENT_BLEND, 0.5f, blendTime, Time.deltaTime);
            }
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

    public void SetActiveGun(AbstractGun weapon)
    {
        // in the animator controller we'll have to be smart about which int lines up with which gun as we can't pass in the enum
        animPlayer.SetInteger(GUN_TYPE, (int) weapon.gunData.type);

        animGun = weapon.GetComponentInChildren<Animator>();
    }
}
