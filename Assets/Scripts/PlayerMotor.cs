using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    public Camera cam;
    private CharacterController controller;

    private float _standingHeight = 2.0f;
    private float _crouchedHeight = 1.0f;

    public float _gravity = -9.8f;
    public float _jumpHeight = 3f;
    public float _walkSpeed = 3f;
    public float _sprintSpeed = 6f;
    public float _slideFrictionDecel = 12f;

    private Vector3 _playerVelocity;

    private Vector3 _prevPosition;
    private Vector3 referenceObjectPosition;

    public bool _isCrouched;
    private bool _isSliding = false;
    public bool _isSprinting;
    public bool _isGrounded;


    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        controller.height = _standingHeight;
        _prevPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
    }

    // receives inputs from input manager script and applies to controller
    // actually happening inside a FixedUpdate
    public void ProcessMove(Vector2 input)
    {
        Vector3 addedVelocity = Vector3.zero;
        Vector3 moveDirection = new Vector3(input.x, 0, input.y);

        _isGrounded = controller.isGrounded;

        // possible player states
        if (_isGrounded)
        {
            // we don't want gravity to continually crank up our negative vert velocity up so reset to some small negative value
            ResetVerticalVelocity();

            if (!_isCrouched)
            {
                // grounded
                addedVelocity += HandleGroundedMovement(moveDirection);
            } else
            {
                // crouched
                addedVelocity += HandleCrouchedMovement(moveDirection);
            }
        } else
        {
            // airborn
        }

        // gravity
        addedVelocity += new Vector3(0, _gravity * Time.deltaTime, 0);

        // move player

        HandleCrouchHeightChange();


        ProcessCharacterMovement(moveDirection);
        ProcessPhysics(moveDirection);

    }

    private Vector3 HandleGroundedMovement(Vector3 moveDirection)
    {
        Vector3 addVelocity;

        // need to ostensibly "stop" our horizontal velocity because we're only going to be moving exactly as far as this method wants
        _playerVelocity.x = 0;
        _playerVelocity.z = 0;

        if (moveDirection.z > 0 && (int)moveDirection.magnitude == 1)
        {
            _isSprinting = true;
        }
        else
        {
            _isSprinting = false;
        }

        if (_isSprinting)
        {
            float horizontalSpeed = new Vector3(_playerVelocity.x, 0, _playerVelocity.z).magnitude;
            float newSpeed = MotionCurves.LinearInterp(horizontalSpeed, _walkSpeed, _sprintSpeed, 1f);
            addVelocity = moveDirection * newSpeed;
        }
        else
        {
            addVelocity = moveDirection * _walkSpeed;
        }

        return addVelocity;
    }

    private Vector3 HandleCrouchedMovement(Vector3 moveDirection)
    {
        Vector3 addVelocity;

        if (!_isSliding)
        {
            // crouch walking

            // need to ostensibly "stop" our horizontal velocity because we're only going to be moving exactly as far as this method wants
            _playerVelocity.x = 0;
            _playerVelocity.z = 0;

            addVelocity = moveDirection * _walkSpeed;
        } else
        {        
            Vector3 frictionUnitVector = -1 * _playerVelocity / _playerVelocity.magnitude;

            addVelocity = frictionUnitVector * _slideFrictionDecel * Time.deltaTime;
        }

        return addVelocity;
    }

    // -----------------------Handle CharacterController-based movement---------------------------------

    private void ProcessCharacterMovement(Vector3 moveDirection)
    {
        if (_isGrounded)
        {

            if (!_isCrouched)
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

    }

    private void HandleCrouched(Vector3 moveDirection)
    {

    }

    // --------------------------Handle physics applied to player-------------------------------------------

    private void ProcessPhysics(Vector3 moveDirection)
    {
        CalcPlayerSliding();
        if (_isGrounded)
        {
            if (!_isCrouched)
            {
                HandleGroundedPhysics();
            } else
            {
                if (_isSliding)
                {
                    HandleCrouchedPhysics(moveDirection);
                } else
                {
                    _playerVelocity.x = 0;
                    _playerVelocity.z = 0;
                }
            }
        } else
        {
            HandleAirbornPhysics(moveDirection);
        }

        controller.Move(_playerVelocity * Time.deltaTime);

        UpdatePlayerVelocity();
    }

    private void HandleGroundedPhysics()
    {

    }

    private void HandleCrouchedPhysics(Vector3 moveDirection)
    {
    }

    private void HandleAirbornPhysics(Vector3 moveDirection)
    {
        if (moveDirection.magnitude > 0)
        {

            // x-plane movement
            Vector3 xDirection = transform.TransformDirection(new Vector3(moveDirection.x, 0, 0));
            _playerVelocity += AddVelocityInDirection(_playerVelocity, xDirection, 15.0f, _walkSpeed);

            // z-plane movement
            Vector3 zDirection = transform.TransformDirection(new Vector3(0, 0, moveDirection.z));
            _playerVelocity += AddVelocityInDirection(_playerVelocity, zDirection, 15.0f, _walkSpeed);

        }

        _playerVelocity.y += _gravity * Time.deltaTime;
    }

    private void UpdatePlayerVelocity()
    {
        Vector3 relativePositionDiff = (transform.position) - (_prevPosition);
        _playerVelocity = relativePositionDiff / Time.deltaTime;
        _prevPosition = transform.position;
    }

    // ---------------------------Util Methods--------------------------------------------------------------

    private void ResetVerticalVelocity()
    {
        if (_playerVelocity.y <= 0.2)
        {
            _playerVelocity.y = -0.5f;
        }
    }

    private Vector3 AddVelocityInDirection(Vector3 currentVelocity, Vector3 direction, float acceleration, float maxVelocity)
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
        if (_isGrounded)
        {
            _playerVelocity.y = Mathf.Sqrt(-2 * _gravity * _jumpHeight);
        }
    }

    private void CalcPlayerSliding()
    {
        float slideCutoffVelocity = 0.2f;

        if (_isCrouched && _isGrounded)
        {
            if (_playerVelocity.magnitude >= _sprintSpeed * 0.95f)
            {
                if (_isSliding == false)
                {
                    float slideBoost = 5f;
                    Vector3 slideBoostVector = _playerVelocity / _playerVelocity.magnitude * slideBoost;
                    _playerVelocity += slideBoostVector;
                }
                _isSliding = true;
            } else if (_playerVelocity.magnitude < slideCutoffVelocity)
            {
                _isSliding = false;
            }

        } else if (_isGrounded)
        {
            _isSliding = false;
        }
    }

    private void HandleCrouchHeightChange()
    {
       float crouchTime = 0.25f;
        float newHeight = 0f;

       if (_isCrouched)
        {
            if (controller.height != _crouchedHeight)
            {
                newHeight = MotionCurves.LinearInterp(controller.height, _standingHeight, _crouchedHeight, crouchTime);
            }
        } else
        {
            if (controller.height != _standingHeight)
            {
                newHeight = MotionCurves.LinearInterp(controller.height, _crouchedHeight, _standingHeight, crouchTime);
            }
        }
        // ensure we've actually updated to a new height
        if (newHeight != 0)
        {
            controller.center = Vector3.down * (_standingHeight - newHeight) / 2f;
            controller.height = newHeight;

            // hardcoded camera height (awful). will fix with model animations
            cam.transform.localPosition = new Vector3(0 , 1.4f * newHeight / _standingHeight - 0.8f, 0);
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
