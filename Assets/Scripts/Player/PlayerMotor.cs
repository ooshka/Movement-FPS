using System.Collections.Generic;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    private Camera cam;
    private CharacterController controller;
    private WallCollisionCheck wallCollisionCheck;
    private StateController stateController;
    private readonly float _standingHeight = 2f;
    private readonly float _crouchedHeight = 1f;

    [Header("Jumping")]
    [SerializeField]
    private float _gravity = -17f;
    [SerializeField]
    private float _jumpHeight = 1.2f;
    [SerializeField]
    private float _jumpCooldown = 0.25f;
    [SerializeField]
    private float _lateJumpDelay = 0.1f;
    [SerializeField]
    [Tooltip("Horizontal rebound velocity")]
    private float _wallJumpReboundVelocity = 2f;

    [Header("Climbing")]
    [SerializeField]
    private float _climbCooldown = 0.5f;
    [SerializeField]
    private int _numOfClimbs = 3;
    [SerializeField]
    private float _climbHeight = 0.9f;
    [SerializeField]
    [Tooltip("The amount of time after a jump that we can initiate a climb")]
    private float _climbJumpDelay = 0.05f;


    [Header("Vaulting")]
    [SerializeField]
    [Tooltip("We are allowed to vault below this velocity")]
    private float _vaultVelocityCutoff = 2f;
    [SerializeField]
    [Tooltip("Total time taken to execute a vault")]
    private float _vaultTime = 1f;
    [SerializeField]
    [Tooltip("Vertical component of vault velocity, will have to be dialed in to make sure we get enough height")]
    private float _vaultVelocityVertical = 2.4f;

    private float _vaultTimer = 0f;

    [Header("Grounded Movement")]
    [SerializeField]
    private float _walkSpeed = 5f;
    [SerializeField]
    private float _crouchWalkSpeed = 3f;
    [SerializeField]
    private float _sprintSpeed = 7f;
    [SerializeField]
    private float _sprintAccelTime = 0.75f;
    [SerializeField]
    private float _groundCollisionThreshold = 0.2f;

    private float _sprintStrafeSpeed;


    [Header("Airborne Movement")]
    public float _airStrafeAccel = 12f;
    private float _airStrafeMaxVelocity;

    [Header("Sliding")]
    [SerializeField]
    private float _slideCutoffVelocity = 1f;
    [SerializeField]
    private float _slideFrictionDecel = 10f;
    [SerializeField]
    private float _slideBoost = 5.0f;
    [SerializeField]
    private float _slidBoostMaxVelocity = 12f;
    [SerializeField]
    [Tooltip("Multiplied by the slope angle and added to the influence of the slope on sliding acceleration")]
    private float _positiveSlopeSlideFactor = 20f;
    [SerializeField]
    [Tooltip("Multiplied by the slope angle and added to the influence of the slope on sliding acceleration")]
    private float _negativeSlopeSlideFactor = 30f;

    private float _slideStartVelocity;


    [Header("Melee/Punch Boost")]
    [SerializeField]
    private float _meleeDistance = 2f;
    [SerializeField]
    private float _meleeCooldown = 0.5f;
    [SerializeField]
    private float _punchBoost = 5f;
    [SerializeField]
    private float _punchBoostMaxVelocity = 12f;
    [SerializeField]
    [Tooltip("Multiplied by the y velocity of the punch boost")]
    private float _punchBoostYLimiter = 0.7f;

    [Header("Flags")]
    [SerializeField]
    public bool _isCrouched;
    [SerializeField]
    private bool _isJumping;
    [SerializeField]
    private bool _isVaulting;
    [SerializeField]
    private bool _isVaultStarting;
    [SerializeField]
    private bool _isMeleeing;
    [SerializeField]
    private bool _isSliding = false;
    [SerializeField]
    private bool _wasSliding;
    [SerializeField]
    private bool _isSprinting;
    [SerializeField]
    private bool _isGrounded;
    [SerializeField]
    private bool _wasGrounded;
    [SerializeField]
    private bool _secondaryGroundedCheck;

    [SerializeField]
    private Vector3 _playerVelocity;

    private Vector3 _prevPosition;
    private Vector3 _referenceObjectPosition;
    private ControllerColliderHit _lastGroundedHit;

    private int _climbCounter;

    private Dictionary<string, Timer> timers = new Dictionary<string, Timer>();
    private readonly string LATE_JUMP_TIMER = "late_jump";
    private readonly string JUMP_COOLDOWN_TIMER = "jump_cooldown";
    private readonly string MELEE_COOLDOWN_TIMER = "melee_cooldown";
    private readonly string CLIMB_COOLDOWN_TIMER = "climb_cooldown";

    private List<StateController.State> frameState = new List<StateController.State>();

    // Start is called before the first frame update
    void Start()
    {
        cam = transform.GetChild(0).gameObject.GetComponent<Camera>();
        wallCollisionCheck = GetComponent<WallCollisionCheck>();
        stateController = GetComponent<StateController>();

        controller = GetComponent<CharacterController>();
        controller.height = _standingHeight;
        _prevPosition = transform.position;
        _slideStartVelocity = 0.90f * _sprintSpeed;
        _sprintStrafeSpeed = _walkSpeed;
        _airStrafeMaxVelocity = _walkSpeed;

        // init all of our timers
        timers.Add(LATE_JUMP_TIMER, new Timer(_lateJumpDelay, true));
        timers.Add(JUMP_COOLDOWN_TIMER, new Timer(_jumpCooldown, false));
        timers.Add(MELEE_COOLDOWN_TIMER, new Timer(_meleeCooldown, false));
        timers.Add(CLIMB_COOLDOWN_TIMER, new Timer(_climbCooldown, false));

        // init our climb counter
        _climbCounter = _numOfClimbs;
    }

    // Update is called once per frame
    void Update()
    {
    }

    // receives inputs from input manager script and applies to controller
    // actually happening inside a FixedUpdate
    public void ProcessMove(Vector2 input)
    {
        // clear the animation state so we can set it up again this frame
        frameState.Clear();

        Vector3 addedVelocity = Vector3.zero;
        Vector3 moveDirection = new Vector3(input.x, 0, input.y);

        // controller.isGrounded is giving poor results
        // utilize a secondary grounded check based on the CapsuleCollider of the CharacterController
        _isGrounded = controller.isGrounded || _secondaryGroundedCheck;

        _secondaryGroundedCheck = false;
        
        if (_isVaulting)
        {
            // experimenting with locking in an actual animation for vaulting
            HandleVaulting(moveDirection);
        } else
        {
            // change the controller's height
            HandleCrouchHeightChange();

            // need to set our sliding flags regardless of state
            CalcPlayerSliding();

            // possible player states
            if (_isGrounded)
            {
                // we don't want gravity to continually crank up our negative vert velocity up so reset to some small negative value
                ResetVerticalVelocity();

                // reset our climb counter
                _climbCounter = _numOfClimbs;

                // we can jump in either crouched or walk/sprint mode
                if (_isJumping && timers[JUMP_COOLDOWN_TIMER].CanTriggerEvent())
                {
                    frameState.Add(StateController.State.JUMPING);
                    addedVelocity += HandleJump(_jumpHeight);
                    // added so we don't get a jump and climb boost at the same time
                    timers[CLIMB_COOLDOWN_TIMER].AddTime(_climbJumpDelay);
                }

                if (!_isCrouched)
                {
                    // grounded
                    addedVelocity += HandleGroundedMovement(moveDirection);
                }
                else
                {
                    // crouched
                    addedVelocity += HandleCrouchedMovement(moveDirection);
                }
            }
            else
            {
                // airborn
                addedVelocity += HandleAirbornMovement(moveDirection);
            }

            // handle melee behaviour
            // breaks state machine a bit cause we need to check individual states within this,
            // but it happens across lots of states so who's to say what's best
            if (_isMeleeing)
            {
                addedVelocity += HandleMelee();
            }

            // gravity
            addedVelocity += new Vector3(0, _gravity * Time.deltaTime, 0);
        }

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

        // store whether we were grounded this frame or not
        _wasGrounded = _isGrounded;

        // iterate all of our timers
        foreach(Timer timer in timers.Values)
        {
            timer.Iterate(Time.deltaTime);
        }

        // update our public animation state
        stateController.SetPlayerState(frameState);
    }

    private Vector3 HandleGroundedMovement(Vector3 moveDirection)
    {
        Vector3 addVelocity = Vector3.zero;

        if (moveDirection.z > 0 && (int)moveDirection.magnitude == 1)
        {
            _isSprinting = true;
            frameState.Add(StateController.State.SPRINTING);
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
            if (moveDirection.magnitude == 0)
            {
                frameState.Add(StateController.State.IDLE);
            } else
            {
                frameState.Add(StateController.State.WALKING);
            }
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
            if (moveDirection.magnitude == 0)
            {
                frameState.Add(StateController.State.CROUCH_IDLE);
            } else
            {
                frameState.Add(StateController.State.CROUCH_WALKING);
            }

            addVelocity += transform.TransformDirection(moveDirection) * _crouchWalkSpeed;

            // need to ostensibly "stop" our horizontal velocity because we're only going to be moving exactly as far as this method wants
            _playerVelocity.x = 0;
            _playerVelocity.z = 0;
        } else
        {
            frameState.Add(StateController.State.SLIDING);
                    
            Vector3 velDirection = _playerVelocity / _playerVelocity.magnitude;

            // we be boostin
            if (_wasSliding == false)
            {
                addVelocity += AddVelocityInDirection(_playerVelocity, velDirection, _slideBoost, _slidBoostMaxVelocity);
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

        frameState.Add(StateController.State.AIRBORNE);

        Vector3 addedVelocity = Vector3.zero;

        if (moveDirection.magnitude > 0)
        {
            // x-plane movement
            Vector3 xDirection = transform.TransformDirection(new Vector3(moveDirection.x, 0, 0));
            addedVelocity += AddVelocityInDirection(_playerVelocity, xDirection, _airStrafeAccel * Time.deltaTime, _airStrafeMaxVelocity);

            // z-plane movement
            Vector3 zDirection = transform.TransformDirection(new Vector3(0, 0, moveDirection.z));
            addedVelocity += AddVelocityInDirection(_playerVelocity, zDirection, _airStrafeAccel * Time.deltaTime, _airStrafeMaxVelocity);

            // if we're holding w
            if (moveDirection.z > 0)
            {
                // going to try and do a velocity limit check so we aren't raycasting every frame
                float forwardVelocity = Vector3.Dot(_playerVelocity, cam.transform.forward);
                if (forwardVelocity <= _vaultVelocityCutoff)
                {
                    if (wallCollisionCheck.CanVault())
                    {
                        Debug.Log("HERE");
                        _isVaulting = true;
                        _isVaultStarting = true;
                    }
                    else
                    {
                        // we need to handle climb animation and our velocity change
                        if (wallCollisionCheck.CanClimb())
                        {
                            if (_climbCounter > 0 && timers[CLIMB_COOLDOWN_TIMER].CanTriggerEventAndReset())
                            {
                                addedVelocity += HandleJump(_climbHeight);
                                _climbCounter--;
                                frameState.Add(StateController.State.CLIMBING);
                            }
                            // if our cooldown timer is currently running it means we're climbing
                            else if (!timers[CLIMB_COOLDOWN_TIMER].CanTriggerEvent() && _climbCounter < _numOfClimbs)
                            {
                                frameState.Add(StateController.State.CLIMBING);
                            }
                        }

                    }
                }
            }

        }

        // also need to see if we can still late jump
        if (_wasGrounded)
        {
            timers[LATE_JUMP_TIMER].Reset();
        }

        if (_isJumping && timers[JUMP_COOLDOWN_TIMER].CanTriggerEvent())
        {
            // wall jump
            if (wallCollisionCheck.CanWallJump())
            {
                addedVelocity += HandleWallJump();
            }
            // late jump
            else if (timers[LATE_JUMP_TIMER].CanTriggerEvent())
            {
                addedVelocity += HandleJump(_jumpHeight);
            }
        }

        return addedVelocity;
    }

    private Vector3 HandleJump(float jumpHeight)
    {
        Vector3 addedVelocity = Vector3.zero;
        addedVelocity.y = Mathf.Sqrt(-2 * _gravity * jumpHeight);
        timers[JUMP_COOLDOWN_TIMER].Reset();
        return addedVelocity;
    }

    private Vector3 HandleWallJump()
    {
        Vector3 addedVelocity = Vector3.zero;
        Vector3 wallJumpVelocity = HandleJump(_jumpHeight);
        // only give upwards velocity if we're below or at our wall jump velocity
        float velocityToAdd = Mathf.Clamp(wallJumpVelocity.y - _playerVelocity.y, 0, wallJumpVelocity.y);
        wallJumpVelocity.y = velocityToAdd;
        addedVelocity += wallJumpVelocity;

        addedVelocity += wallCollisionCheck.getLastHitNormal() * _wallJumpReboundVelocity;

        return addedVelocity;
    }

    private Vector3 HandleMelee()
    {
        frameState.Add(StateController.State.MELEE);

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
                    addedVelocity += AddVelocityInDirection(_playerVelocity, -cam.transform.forward.normalized, _punchBoost, _punchBoostMaxVelocity);
                    addedVelocity.y *= _punchBoostYLimiter;
                    if (_isGrounded && _isCrouched)
                    {
                        _isSliding = true;
                    }
                }
            }
        }
        return addedVelocity;
    }

    private void HandleVaulting(Vector3 moveDirection)
    {
        if (_vaultTimer >= _vaultTime)
        {
            _isVaulting = false;
            _vaultTimer = 0f;
            return;
        }

        // we might need to cancel the vault if we're no longer touching the ledge
        // but only do so if we aren't almost all the way through the animation
        if (!wallCollisionCheck.CanContinueVaulting() && _vaultTimer / _vaultTime < 0.9)
        {
            frameState.Add(StateController.State.VAULT_CANCEL);
            _isVaulting = false;
            _vaultTimer = 0f;
            return;
        }

        // advance our timer which determines our transition through the vault animation
        _vaultTimer += Time.deltaTime;
        // reset our vaulting params
        if (_isVaultStarting)
        {
            _vaultTimer = 0;
            _isVaultStarting = false;

            frameState.Add(StateController.State.VAULTING);
        }

        float t = _vaultTimer;
        float c = 2 * Mathf.PI / (_vaultTime / 2f);
        float v = _vaultVelocityVertical / 2f;

        float y_velocity = v - v * Mathf.Cos(c * t);

        // directly assign velocity rather than giving additive pieces
        _playerVelocity.y = y_velocity;
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
        if (_playerVelocity.y <= 0.5 || _playerVelocity.y > 0)
        {
            _playerVelocity.y = -0.4f;
        }
    }

    private Vector3 AddVelocityInDirection(Vector3 currentVelocity, Vector3 direction, float additionalVelocity, float maxVelocity)
    {
        float velInMoveDirection = Vector3.Dot(direction, currentVelocity) / direction.magnitude;

        velInMoveDirection = Mathf.Max(0, velInMoveDirection);

        if (velInMoveDirection + additionalVelocity >= maxVelocity)
        {
            additionalVelocity = Mathf.Max(0, maxVelocity - velInMoveDirection);
        }

        return additionalVelocity * direction;
    }

    public void Jump()
    {
        // set a flag here so we know we're jumping and can set proper velocity in normal flow
        _isJumping = true;  
    }

    public void Melee()
    {
        if (timers[MELEE_COOLDOWN_TIMER].CanTriggerEventAndReset())
        {
            _isMeleeing = true;

        }
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

        if (_isGrounded && _lastGroundedHit != null && !_isJumping && !_isMeleeing)
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
            _referenceObjectPosition = transform.position;
            _lastGroundedHit = hit;
            _secondaryGroundedCheck = true;
        }
    }
}
