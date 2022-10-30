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
        prevPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = controller.isGrounded;
    }

    // receives inputs from input manager script and applies to controller
    // actually happening inside a FixedUpdate
    public void ProcessMove(Vector2 input)
    {
        if (isGrounded)
        {
            horizontalSpeed = new Vector3(playerVelocity.x, 0, playerVelocity.z).magnitude;
        }
        HandleCrouchHeightChange();

        Vector3 moveDirection = new Vector3(input.x, 0, input.y);

        ProcessCharacterMovement(moveDirection);
        ProcessPhysics(moveDirection);

    }

    // -----------------------Handle CharacterController-based movement---------------------------------

    private void ProcessCharacterMovement(Vector3 moveDirection)
    {
        if (isGrounded)
        {
            if (!isCrouched)
            {
                HandleGrounded(moveDirection);
            }
            else
            {
                HandleCrouched(moveDirection);
            }
        }
    }

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

    // --------------------------Handle physics applied to player-------------------------------------------

    private void ProcessPhysics(Vector3 moveDirection)
    {
        if (isGrounded)
        {
            HandleGroundedPhysics();
        } else
        {
            HandleAirbornPhysics(moveDirection);
        }

        controller.Move(playerVelocity * Time.deltaTime);

        UpdatePlayerVelocity();
    }

    private void HandleGroundedPhysics()
    {
        playerVelocity.x = 0;
        playerVelocity.z = 0;

        if (playerVelocity.y <= 0)
        {
            playerVelocity.y = -0.5f;
        } else
        {
            // this is here in case we are techincally "grounded" with a positive velocity
            // might apply gravity to jump twice so may need to redo
            playerVelocity.y += gravity * Time.deltaTime;
        }
    }

    private void HandleAirbornPhysics(Vector3 moveDirection)
    {
        float airStrafeAccel = 10.0f;

        playerVelocity += airStrafeAccel * transform.TransformDirection(moveDirection) * Time.deltaTime;

        playerVelocity.y += gravity * Time.deltaTime;
    }

    private void UpdatePlayerVelocity()
    {
        Vector3 relativePositionDiff = (transform.position) - (prevPosition);
        playerVelocity = relativePositionDiff / Time.deltaTime;
        prevPosition = transform.position;
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
            referenceObjectPosition = transform.position;
        }
    }


}
