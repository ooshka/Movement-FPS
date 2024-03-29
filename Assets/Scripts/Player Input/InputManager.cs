using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerInput.GroundedActions groundedActions;
    private PlayerInput.ShootingActions shootingActions;
    private PlayerInput.UtilityActions utilityActions;
    private PlayerLook look;
    private PlayerMotor motor;
    public static Action reloadAction, shootStartAction, shootEndAction, adsStartAction, adsEndAction, interactAction;

    // Start is called before the first frame update
    void Awake()
    {
        playerInput = new PlayerInput();
        groundedActions = playerInput.Grounded;
        shootingActions = playerInput.Shooting;
        utilityActions = playerInput.Utility;
        motor = GetComponent<PlayerMotor>();
        look = GetComponent<PlayerLook>();
        
        // grounded actions

        // TODO: transfer these over to Actions so we can set up proper listeners and not need reference to the actual classes
        groundedActions.Jump.performed += ctx => motor.Jump();
        groundedActions.Melee.performed += ctx => motor.Melee();
        groundedActions.Crouch.started += ctx => motor._isCrouched = true;
        groundedActions.Crouch.canceled += ctx => motor._isCrouched = false;

        // shooting
        shootingActions.Shoot.started += ctx => shootStartAction?.Invoke();
        shootingActions.Shoot.canceled += ctx => shootEndAction?.Invoke();
        shootingActions.Reload.performed += ctx => reloadAction?.Invoke();
        shootingActions.ADS.started += ctx => adsStartAction?.Invoke();
        if (!DebugFlags.IsAlwaysADS())
        {
            shootingActions.ADS.canceled += ctx => adsEndAction?.Invoke();
        }

        // utility
        utilityActions.Interact.performed += ctx => interactAction?.Invoke();
    }

    void FixedUpdate()
    {
        // tell the motor to move using our grounded action
        Vector2 movement = groundedActions.Movement.ReadValue<Vector2>();
        // TODO: move logic to PlayerMotor
        motor.ProcessMove(movement);
    }

    private void LateUpdate()
    {
        look.ProcessLook(groundedActions.Look.ReadValue<Vector2>());
    }

    private void OnEnable()
    {
        groundedActions.Enable();
        shootingActions.Enable();
        utilityActions.Enable();
    }

    private void OnDisable()
    {
        groundedActions.Disable();
        shootingActions.Disable();
        utilityActions.Disable();
    }
}
