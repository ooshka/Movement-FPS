using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerInput.GroundedActions groundedActions;
    private PlayerLook look;
    private PlayerMotor motor;
    // Start is called before the first frame update
    void Awake()
    {
        playerInput = new PlayerInput();
        groundedActions = playerInput.Grounded;
        motor = GetComponent<PlayerMotor>();
        look = GetComponent<PlayerLook>();
        groundedActions.Jump.performed += ctx => motor.Jump();
        groundedActions.Crouch.started += ctx => motor._isCrouched = true;
        groundedActions.Crouch.canceled += ctx => motor._isCrouched = false;
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
    }

    private void OnDisable()
    {
        groundedActions.Disable();
    }
}
