using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    public Camera cam;
    private CharacterController controller;
    private Vector3 playerVelocity;

    private Vector3 prevPosition;
    private Vector3 referenceObjectPosition;

    private float standingHeight = 2.0f;
    private float crouchedHeight = 1.0f;

    private float horizontalSpeed;
    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float slideBoost = 1.5f;
    public float crouchStrafeDamping = 0.2f;
    public float airStrafeDamping = 0.5f;

    public bool isCrouched;
    public bool isSprinting;
    private bool isGrounded;

    public float gravity = -9.8f;
    public float jumpHeight = 3f;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        controller.height = standingHeight;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = controller.isGrounded;
    }

    private void FixedUpdate()
    {
        if (isGrounded)
        {
            Vector3 relativePositionDiff = (transform.position - referenceObjectPosition) - (prevPosition - referenceObjectPosition);
            horizontalSpeed = new Vector3(relativePositionDiff.x, 0, relativePositionDiff.z).magnitude / Time.deltaTime;
        }
        prevPosition = transform.position;
        HandleCrouchHeightChange();
    }

    // receives inputs from input manager script and applies to controller
    // actually happening inside a FixedUpdate
    public void ProcessMove(Vector2 input)
    {
        Vector3 moveDirection = new Vector3(input.x, 0, input.y);
        if (isGrounded)
        {
            if (!isCrouched)
            {
                HandleGrounded(moveDirection);
            } else
            {
                HandleCrouched(moveDirection);
            }
        }
        else
        {
            HandleAirborn(moveDirection);
        }

        ProcessPhysics();
    }

    // -----------------------Handle CharacterController-based movement---------------------------------

    private void HandleGrounded(Vector3 moveDirection)
    {
        if (isSprinting)
        {
            float newSpeed = MotionCurves.LinearInterp(horizontalSpeed, walkSpeed, sprintSpeed, 1f);
            controller.Move(transform.TransformDirection(moveDirection * newSpeed * Time.deltaTime));
        }
        else
        {
            controller.Move(transform.TransformDirection(moveDirection * walkSpeed * Time.deltaTime));
        }
    }

    private void HandleCrouched(Vector3 moveDirection)
    {
        if (horizontalSpeed >= sprintSpeed * 0.95f)
        {
            // Sliding
            // Will need to add a handlePhysics method to compute competing forces on the player
            if (horizontalSpeed >= sprintSpeed * slideBoost)
            {
                // we're already zooming no need to give a boost for sliding
                controller.Move(transform.TransformDirection(moveDirection * horizontalSpeed * Time.deltaTime));
            }
            else
            {
                controller.Move(transform.TransformDirection(moveDirection * sprintSpeed * slideBoost * Time.deltaTime));
            }

        }
        else
        {
            // Crouch walking
            controller.Move(transform.TransformDirection(moveDirection * walkSpeed * Time.deltaTime));
        }
    }

    private void HandleAirborn(Vector3 moveDirection)
    {
        float airSpeed = Mathf.Max(horizontalSpeed, walkSpeed);
        controller.Move(transform.TransformDirection(moveDirection * airSpeed * Time.deltaTime));
    }

    // --------------------------Handle physics applied to player-------------------------------------------

    private void ProcessPhysics()
    {
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -.2f;
        } else
        {
            playerVelocity.y += gravity * Time.deltaTime;
        }
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void HandleAirbornPhysics()
    {

    }

    // ---------------------------Util Methods--------------------------------------------------------------
    public void Jump()
    {
        if (isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(-2 * gravity * jumpHeight);
        }
    }

    private void HandleCrouchHeightChange()
    {
       float crouchTime = 0.25f;
        float newHeight = 0f;

       if (isCrouched)
        {
            if (controller.height != crouchedHeight)
            {
                newHeight = MotionCurves.LinearInterp(controller.height, standingHeight, crouchedHeight, crouchTime);
            }
        } else
        {
            if (controller.height != standingHeight)
            {
                newHeight = MotionCurves.LinearInterp(controller.height, crouchedHeight, standingHeight, crouchTime);
            }
        }
        // ensure we've actually updated to a new height
        if (newHeight != 0)
        {
            controller.center = Vector3.down * (standingHeight - newHeight) / 2f;
            controller.height = newHeight;

            // hardcoded camera heiht (awful). will fix with model animations
            cam.transform.localPosition = new Vector3(0 , 1.4f * newHeight / standingHeight - 0.8f, 0);
        }
    }

    public void OnControllerColliderHit(ControllerColliderHit hit)
    {
        float groundCollisionThreshold = 0.1f;

        Collider collider = GetComponent<Collider>();
        float playerCenterY = collider.bounds.center.y;
        float playerExtentY = collider.bounds.extents.y;

        // check to see if the object we hit is the one we are standing on
        // do this by seeing if the y coordinate of the collision matches up with the bottom of the capsule collider
        if (Mathf.Abs(playerCenterY - playerExtentY - hit.point.y) < groundCollisionThreshold)
        {
            referenceObjectPosition = hit.transform.position;
        }
    }


}
