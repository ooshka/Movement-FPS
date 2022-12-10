using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    public Camera cam;
    private CharacterController controller;
    private float _standingHeight = 1.4f;
    private float _crouchedHeight = 0.7f;

    public float _gravity = -15f;
    public float _jumpHeight = 1.2f;
    public float _walkSpeed = 4f;
    public float _sprintSpeed = 8f;
    public float _sprintStrafeSpeed = 4f;
    public float _sprintAccelTime = 0.75f;
    public float _airStrafeAccel = 10f;
    public float _airStrafeMaxVelocity = 8f;
    public float _slideCutoffVelocity = 0.2f;
    public float _slideStartVelocity;
    public float _slideFrictionDecel = 12f;
    public float _slideBoost = 4.0f;
    public float _positiveSlopeSlideFactor = 20f;
    public float _negativeSlopeSlideFactor = 40f;
    public float _groundCollisionThreshold = 0.2f;
    public float _meleeDistance = 2f;
    public float _punchBoost = 8f;

    public bool _isCrouched;
    public bool _isJumping;
    public bool _isMeleeing;
    public bool _isSliding = false;
    public bool _wasSliding;
    public bool _isSprinting;
    public bool _isGrounded;
    public bool _secondaryGroundedCheck;

    public Vector3 _playerVelocity;

    private Vector3 _prevPosition;
    private Vector3 referenceObjectPosition;
    private ControllerColliderHit _lastGroundedHit;


    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        controller.height = _standingHeight;
        _prevPosition = transform.position;
        _slideStartVelocity = 0.95f * _sprintSpeed;
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

        // controller.isGrounded is giving poor results
        // utilize a secondary grounded check based on the CapsuleCollider of the CharacterController
        _isGrounded = controller.isGrounded || _secondaryGroundedCheck;

        _secondaryGroundedCheck = false;
        
        // change the controller's height
        HandleCrouchHeightChange();

        // need to set our sliding flags regardless of state
        CalcPlayerSliding();

        // handle melee behaviour
        // breaks state machine a bit cause we need to check individual states within this,
        // but it happens across lots of states so who's to say what's best
        if (_isMeleeing)
        {
            addedVelocity += HandleMelee();
        }

        // possible player states
        if (_isGrounded)
        {
            // we don't want gravity to continually crank up our negative vert velocity up so reset to some small negative value
            ResetVerticalVelocity();

            // we can jump in either crouched or walk/sprint mode
            if (_isJumping)
            {
                addedVelocity.y += Mathf.Sqrt(-2 * _gravity * _jumpHeight);
            }

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
            addedVelocity += HandleAirbornMovement(moveDirection);
        }

        // gravity
        addedVelocity += new Vector3(0, _gravity * Time.deltaTime, 0);

        // add our new velocity
        _playerVelocity += addedVelocity;

        // right before we move we need to figure out if we'll be moving down a slope
        // if so we should add some negative vert velocity so that we "suck" to the slope
        _playerVelocity.y += DownwardSlopeSucker();

        // move player
        controller.Move(_playerVelocity * Time.deltaTime);

        // update global velocity
        UpdatePlayerVelocity();

        // reset our various flags
        _isJumping = false;
        _isMeleeing = false;

    }

    private Vector3 HandleGroundedMovement(Vector3 moveDirection)
    {
        Vector3 addVelocity = Vector3.zero;

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
            float newSpeed = MotionCurves.LinearInterp(horizontalSpeed, _walkSpeed, _sprintSpeed, _sprintAccelTime);


            float xMoveComponent = moveDirection.x;
            float zMoveComponent = moveDirection.z;

            if (moveDirection.x != 0)
            {
                // if we have a/d pressed we don't want a full sprint in the diagonal, 
                // so skew the unit vector direction based on the max sprint strafe speed

                // end term is just to preserve the sign of the original input
                xMoveComponent = _sprintStrafeSpeed / _sprintSpeed * xMoveComponent/Mathf.Abs(xMoveComponent);

                // now we need to find the z component that will preserve unit-ness of the vector
                // no need to preserve sign here cause we can only sprint forward
                zMoveComponent = Mathf.Sqrt(1 - Mathf.Pow(xMoveComponent, 2));
            }
            addVelocity += transform.TransformDirection(new Vector3(xMoveComponent, 0, zMoveComponent)) * newSpeed;
        }
        else
        {
            addVelocity += transform.TransformDirection(moveDirection) * _walkSpeed;
        }

        // need to ostensibly "stop" our horizontal velocity because we're only going to be moving exactly as far as this method wants
        _playerVelocity.x = 0;
        _playerVelocity.z = 0;

        return addVelocity;
    }

    private Vector3 HandleCrouchedMovement(Vector3 moveDirection)
    {
        Vector3 addVelocity = Vector3.zero;

        if (!_isSliding)
        {
            // crouch walking

            addVelocity += transform.TransformDirection(moveDirection) * _walkSpeed;

            // need to ostensibly "stop" our horizontal velocity because we're only going to be moving exactly as far as this method wants
            _playerVelocity.x = 0;
            _playerVelocity.z = 0;
        } else
        {
            Vector3 velDirection = _playerVelocity / _playerVelocity.magnitude;

            // we be boostin
            if (_wasSliding == false)
            {
                Vector3 _slideBoostVelocity = velDirection * _slideBoost;
                addVelocity += _slideBoostVelocity;
            }

            // let the slope boost or impede us based on angle
            float slopeInfluence = 0;
            if (_lastGroundedHit != null)
            {
                Vector3 slopeNormal = _lastGroundedHit.normal;
                // only really worry about our horizontal velocity
                Vector3 horizontalVelocity = new Vector3(_playerVelocity.x, 0, _playerVelocity.z);
                // take the negative of this so that uphill gives a positive value and downhill gives negative
                float cosTheta = -1 * Vector3.Dot(horizontalVelocity, slopeNormal) / (horizontalVelocity.magnitude * slopeNormal.magnitude);
                if (cosTheta > 0)
                {
                    slopeInfluence = cosTheta * _positiveSlopeSlideFactor;
                } else
                {
                    slopeInfluence = cosTheta * _negativeSlopeSlideFactor;
                }
                  
            }
            float frictionAmount = (_slideFrictionDecel + slopeInfluence) * Time.deltaTime;

            Vector3 frictionUnitVector = -1 * velDirection;
            addVelocity += frictionUnitVector * frictionAmount;
        }

        return addVelocity;
    }

    private Vector3 HandleAirbornMovement(Vector3 moveDirection)
    {
        Vector3 addVelocity = Vector3.zero;

        if (moveDirection.magnitude > 0)
        {

            // x-plane movement
            Vector3 xDirection = transform.TransformDirection(new Vector3(moveDirection.x, 0, 0));
            addVelocity += AddVelocityInDirection(_playerVelocity, xDirection, _airStrafeAccel, _airStrafeMaxVelocity);

            // z-plane movement
            Vector3 zDirection = transform.TransformDirection(new Vector3(0, 0, moveDirection.z));
            addVelocity += AddVelocityInDirection(_playerVelocity, zDirection, _airStrafeAccel, _airStrafeMaxVelocity);

        }

        return addVelocity;
    }

    private Vector3 HandleMelee()
    {
        Vector3 addedVelocity = Vector3.zero;
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, _meleeDistance))
        {
            if (hit.collider != null)
            {
                // TODO: deal with regular damaging tings
                // maybe we even have seperate punch boost/melee attack buttons

                // only want to punch boost if we are crouched or airborne
                if (!_isGrounded || (_isGrounded && _isCrouched))
                {
                    addedVelocity += (-cam.transform.forward.normalized * _punchBoost);
                }
            }
        }
        return addedVelocity;
    }

    // ---------------------------Util Methods--------------------------------------------------------------

    private void UpdatePlayerVelocity()
    {
        Vector3 relativePositionDiff = (transform.position) - (_prevPosition);
        _playerVelocity = relativePositionDiff / Time.deltaTime;
        _prevPosition = transform.position;
    }

    private void ResetVerticalVelocity()
    {
        // we want to slightly stick to the ground to keep our grounded check alive
        // and also don't want gravity to continually increase our negative y vel if we're grounded

        // also want to not go flying off every little bump so reset y vel if we need to
        if (_playerVelocity.y <= 0.2 || (_playerVelocity.y > 0))
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
            // set a flag here so we know we're jumping and can set proper velocity in normal flow
            _isJumping = true;
        }
    }

    public void Melee()
    {
        _isMeleeing = true;
    }

    private void CalcPlayerSliding()
    {
        // have some handle on the previous frame's slide value so we can boost
        _wasSliding = _isSliding;

        if (_isCrouched && _isGrounded)
        {
            if (_playerVelocity.magnitude >= _slideStartVelocity)
            {
                _isSliding = true;
            } else if (_playerVelocity.magnitude < _slideCutoffVelocity)
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

    private float DownwardSlopeSucker()
    {
        float suckVelocity = 0f;
        float buffer = 1.1f;

        if (_isGrounded && _lastGroundedHit != null && !_isJumping)
        {
            Vector3 slopeNormal = _lastGroundedHit.normal;

            // the amount of our current velocity in the direction of the surface normal
            float velocityNormalComponent = (Vector3.Dot(_playerVelocity, slopeNormal) / slopeNormal.magnitude);

            // if we have some component in normal direction (down the slope)
            if (velocityNormalComponent > 0)
            {
                // the angle between the surface normal we are standing on and the vertical axis (i.e. the slope of the surface)
                float theta = Mathf.Acos(Vector3.Dot(slopeNormal, Vector3.up) / (slopeNormal.magnitude * Vector3.up.magnitude));

                // if we're allowed to walk on it
                if (theta * Mathf.Rad2Deg < controller.slopeLimit)
                {
                    // the amount to suck the player down so they remain on the slope
                    suckVelocity = -1 * velocityNormalComponent / Mathf.Cos(theta);
                }
            }
        }

        return suckVelocity *= buffer;
    }

    public void OnControllerColliderHit(ControllerColliderHit hit)
    {

        Collider collider = GetComponent<Collider>();
        float playerCenterY = collider.bounds.center.y;
        float playerExtentY = collider.bounds.extents.y;

        // check to see if the object we hit is the one we are standing on
        // do this by seeing if the y coordinate of the collision matches up with the bottom of the capsule collider
        if (Mathf.Abs(playerCenterY - playerExtentY - hit.point.y) < _groundCollisionThreshold)
        {
            referenceObjectPosition = transform.position;
            _lastGroundedHit = hit;
            _secondaryGroundedCheck = true;
        }
    }


}
