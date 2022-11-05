using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    public Camera cam;
    private CharacterController controller;

    // TODO: Refactor to local variable
    private Vector3 playerVelocity;

    private Vector3 prevPosition;
    private Vector3 referenceObjectPosition;

    private float standingHeight = 2.0f;
    private float crouchedHeight = 1.0f;

    private float horizontalSpeed;
    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;

    public bool isCrouched;
    private bool isSliding = false;
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
        if (!isSliding)
        {
            // Crouch walking
            controller.Move(transform.TransformDirection(moveDirection * walkSpeed * Time.deltaTime));
        }
    }

    // --------------------------Handle physics applied to player-------------------------------------------

    private void ProcessPhysics(Vector3 moveDirection)
    {
        CalcPlayerSliding();
        if (isGrounded)
        {
            if (playerVelocity.y <= 0.2)
            {
                playerVelocity.y = -0.5f;
            }
            else
            {
                // this is here in case we are techincally "grounded" with a positive velocity
                // might apply gravity to jump twice so may need to redo
                playerVelocity.y += gravity * Time.deltaTime;
            }
            if (!isCrouched)
            {
                HandleGroundedPhysics();
            } else
            {
                if (isSliding)
                {
                    HandleCrouchedPhysics(moveDirection);
                } else
                {
                    playerVelocity.x = 0;
                    playerVelocity.z = 0;
                }
            }
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
    }

    private void HandleCrouchedPhysics(Vector3 moveDirection)
    {
        float frictionAccel = 12f;
        Vector3 frictionUnitVector = -playerVelocity / playerVelocity.magnitude;

        playerVelocity += frictionUnitVector * frictionAccel * Time.deltaTime;
    }

    private void HandleAirbornPhysics(Vector3 moveDirection)
    {
        if (moveDirection.magnitude > 0)
        {

            // x-plane movement
            Vector3 xDirection = transform.TransformDirection(new Vector3(moveDirection.x, 0, 0));
            playerVelocity += AddVelocityInDirection(playerVelocity, xDirection, 15.0f, walkSpeed);

            // z-plane movement
            Vector3 zDirection = transform.TransformDirection(new Vector3(0, 0, moveDirection.z));
            playerVelocity += AddVelocityInDirection(playerVelocity, zDirection, 15.0f, walkSpeed);

        }

        playerVelocity.y += gravity * Time.deltaTime;
    }

    private void UpdatePlayerVelocity()
    {
        Vector3 relativePositionDiff = (transform.position) - (prevPosition);
        playerVelocity = relativePositionDiff / Time.deltaTime;
        prevPosition = transform.position;
    }

    // ---------------------------Util Methods--------------------------------------------------------------


    public Vector3 AddVelocityInDirection(Vector3 currentVelocity, Vector3 direction, float acceleration, float maxVelocity)
    {
        float velInMoveDirection = Vector3.Dot(direction, currentVelocity) / direction.magnitude;

        velInMoveDirection = Mathf.Max(0, velInMoveDirection);

        float additionalVelocity = acceleration * Time.deltaTime;

        if (velInMoveDirection + additionalVelocity >= maxVelocity)
        {
            additionalVelocity = Mathf.Max(0, maxVelocity - velInMoveDirection);
        }

        return additionalVelocity * direction;
    }

    public void Jump()
    {
        if (isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(-2 * gravity * jumpHeight);
        }
    }

    private void CalcPlayerSliding()
    {
        float slideCutoffVelocity = 0.2f;

        if (isCrouched && isGrounded)
        {
            if (playerVelocity.magnitude >= sprintSpeed * 0.95f)
            {
                if (isSliding == false)
                {
                    float slideBoost = 5f;
                    Vector3 slideBoostVector = playerVelocity / playerVelocity.magnitude * slideBoost;
                    playerVelocity += slideBoostVector;
                }
                isSliding = true;
            } else if (playerVelocity.magnitude < slideCutoffVelocity)
            {
                isSliding = false;
            }

        } else if (isGrounded)
        {
            isSliding = false;
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
