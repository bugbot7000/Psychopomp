using System.Collections;
using UnityEngine;

/// <summary>
/// Controls player movement, rotation, jumping, gliding, and environmental interaction.
/// Handles velocity, gravity, coyote time, jump buffering, and tier-based speed control.
/// </summary>
public class KinematicCharacterController : MonoBehaviour
{
    // ====== ROTATION SETTINGS ======
    [Header("Rotation Settings")]
    [SerializeField, Tooltip("Rotation speed for turning around the Y-axis.")]
    private float _rotationStrength = 1f;
    [SerializeField, Tooltip("Strength of pitch rotation while gliding.")]
    private float _yzRotationStrength = 100f;
    [SerializeField, Tooltip("Speed of upward pitch adjustment while gliding.")]
    private float _yzUpRotationSpeed = 100f;
    [SerializeField, Tooltip("Speed of downward pitch adjustment while gliding.")]
    private float _yzDownRotationSpeed = 100f;
    [SerializeField, Tooltip("Speed at which pitch returns to neutral.")]
    private float _yzReturnRotationSpeed = 100f;
    [SerializeField, Tooltip("Maximum allowed pitch angle while gliding.")]
    private float _maxYZRotationAngle = 60f;

    // ====== MOVEMENT SETTINGS ======
    [Header("Movement Settings")]
    [SerializeField] private float _maxSpeed = 100f;
    [SerializeField] private float[] _speedTiers = { 50f, 100f, 150f, 200f };
    [SerializeField] private float _absoluteMaxSpeed = 250f;
    [SerializeField] private float _minSpeed = 10f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _deceleration = 40f;
    [SerializeField] private float _passiveDeceleration = 10f;
    [SerializeField] private float _rampMagnitudeMultiplier = 1.05f;

    // ====== FALLING SETTINGS ======
    [Header("Falling Settings")]
    [SerializeField] private float _gravity = 10f;
    [SerializeField] private float _fallGravityMultiplier = 1.5f;
    [SerializeField] private float _maxFallSpeed = 100f;

    // ====== JUMP SETTINGS ======
    [Header("Jump Settings")]
    [SerializeField, Tooltip("Initial upward velocity when jumping.")]
    private float _jumpVelocity = 10f;
    [SerializeField, Tooltip("Maximum number of allowed jumps (e.g., double jump).")]
    private int _maxJumpCount = 2;
    [SerializeField, Tooltip("Grace period after leaving the ground during which a jump is still allowed.")]
    private float _coyoteTime = 0.2f;
    [SerializeField, Tooltip("Time window to buffer a jump before landing.")]
    private float _jumpBufferTime = 0.1f;

    // ====== GLIDE SETTINGS ======
    [Header("Glide Settings")]
    [SerializeField] private float _glideTimeLimit = 4f;
    [SerializeField] private float _glideGravityFactor = 0.1f;
    [SerializeField] private float _glideMaxFallSpeed = 10f;
    [SerializeField] private float _freezeFrameDuration = 0.1f;
    [SerializeField] private float _glideDeceleration = 20f;
    [SerializeField] private float _glideAcceleration = 20f;

    // ====== STATE VARIABLES ======
    private Vector2 _input;
    private Vector3 _currentNormal;
    private string _currentGroundTag;
    private Vector3 _forwardVector;
    private Vector3 _currentVelocity;
    private float _velocityMagnitude;
    private float _effectiveVelocityMagnitude;
    private int _currentTier;

    private bool _isGrounded;
    private bool _wasGrounded;
    private bool _isBroadlyGrounded;
    private bool _isJumping;
    private bool _isGliding;
    private bool _isFrozen;

    private float _fallingVelocity;
    private float _currentCoyote;
    private float _currentJumpBuffer;
    private int _currentJumpCount;
    private float _glideTimer;

    // ====== COMPONENT REFERENCES ======
    private ColliderUtil _colliderUtil;
    private CharacterVFX _vfx;
    [SerializeField] private WwiseSoundManager _wwiseSoundManager;
    [SerializeField] private CameraController _cameraController;

    // ====== UNITY LIFECYCLE ======
    private void Start()
    {
        _colliderUtil = GetComponent<ColliderUtil>();
        _vfx = GetComponent<CharacterVFX>();
    }

    private void Update()
    {
        if (_isFrozen) return;

        HandleInput();
        HandleRotation();
        HandleGroundChecks();
        HandleJumping();
        HandleGliding();
        HandleMovement();
        ApplyFinalMovement();
    }

    // ====== INPUT ======
    private void HandleInput()
    {
        _input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
    }

    // ====== ROTATION ======
    private void HandleRotation()
    {
        transform.Rotate(Vector3.up * _input.x * _rotationStrength * Time.deltaTime, Space.World);
        ApplyYZRotation(_input);
    }

    private void ApplyYZRotation(Vector2 input)
    {
        Quaternion target = Quaternion.Euler(0, transform.localEulerAngles.y, 0);
        float speed = _yzReturnRotationSpeed;

        if (_isGliding)
        {
            speed = input.y > 0 ? _yzDownRotationSpeed : _yzUpRotationSpeed;
            float xRot = transform.eulerAngles.x + _yzRotationStrength * input.y * Time.deltaTime;
            xRot = ClampAngle(xRot, -_maxYZRotationAngle, _maxYZRotationAngle);
            target = Quaternion.Euler(xRot, transform.localEulerAngles.y, 0f);
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, target, speed * Time.deltaTime);
    }

    // ====== GROUND CHECKS ======
    private void HandleGroundChecks()
    {
        _wasGrounded = _isGrounded;
        _isGrounded = _colliderUtil.IsGroundedCast(transform.position, out _currentNormal, out _currentGroundTag);
        _isBroadlyGrounded = _colliderUtil.BroaderIsGroundedCast(transform.position);

        if (_isGrounded)
        {
            _currentCoyote = _coyoteTime;
            _isJumping = false;
            _isGliding = false;
            _currentJumpCount = 0;
            _glideTimer = 0;
            _fallingVelocity = 0;
            _vfx.PlaySplashSound();
        }
        else
        {
            _currentCoyote -= Time.deltaTime;
            _vfx.StopSplashSound();
            _fallingVelocity = CalculateFallingVelocity();
        }
    }

    // ====== JUMPING ======
    private void HandleJumping()
    {
        if (Input.GetButtonDown("Jump"))
            _currentJumpBuffer = _jumpBufferTime;
        else
            _currentJumpBuffer -= Time.deltaTime;

        bool canGroundJump = _currentCoyote > 0f && _currentJumpBuffer > 0f;
        bool canAirJump = _isJumping && _currentJumpCount < _maxJumpCount && Input.GetButtonDown("Jump");

        if (canGroundJump || canAirJump)
            ExecuteJump();
    }

    private void ExecuteJump()
    {
        _isJumping = true;
        _vfx.PlayJump();
        _currentJumpBuffer = 0;
        _currentCoyote = 0;
        _currentJumpCount++;
        _fallingVelocity = _jumpVelocity;
    }

    // ====== GLIDING ======
    private void HandleGliding()
    {
        bool glideInput = Input.GetButton("Glide") || Input.GetAxis("Glide") > 0;

        if (!_isGrounded && glideInput && !_isGliding && _glideTimer < _glideTimeLimit && _velocityMagnitude > 15f)
            StartGlide();
        else if (!glideInput || _glideTimer >= _glideTimeLimit || _velocityMagnitude < 15f)
            StopGlide();

        if (_isGliding) _glideTimer += Time.deltaTime;
    }

    private void StartGlide()
    {
        _isGliding = true;
        _vfx.PlayFeather();
        _vfx.StartGlide();
        FreezeFrame();
    }

    private void StopGlide()
    {
        if (!_isGliding) return;
        _isGliding = false;
        _vfx.StopGlide();
    }

    // ====== MOVEMENT ======
    private void HandleMovement()
    {
        _velocityMagnitude = CalculateCurrentVelocityMagnitude(_input.y);
        if (_isGliding) _velocityMagnitude = CalculateGlidingAcceleration(_velocityMagnitude);

        _effectiveVelocityMagnitude = Mathf.Min(_speedTiers[_currentTier], _velocityMagnitude);
        _forwardVector = GetForwardVector();
        Vector3 moveDir = _forwardVector * GetForwardVectorMultiplier(_forwardVector);
        Vector3 newVelocity = _effectiveVelocityMagnitude * moveDir;
        newVelocity.y += _fallingVelocity;
        _currentVelocity = newVelocity;
    }

    private float CalculateCurrentVelocityMagnitude(float inputY)
    {
        if (!_isGrounded) return _velocityMagnitude;

        float acc = 0f;
        float dec = 0f;

        if (inputY > 0) acc = _acceleration;
        else if (inputY < 0) dec = _deceleration;
        else dec = _passiveDeceleration;

        float vel = _velocityMagnitude + (acc - dec) * Time.deltaTime;
        return Mathf.Clamp(vel, _minSpeed, _absoluteMaxSpeed);
    }

    private Vector3 GetForwardVector()
    {
        Vector3 fwd = transform.forward;
        if (_isGrounded) fwd.y = 0;
        return fwd.normalized;
    }

    private float GetForwardVectorMultiplier(Vector3 fwd)
    {
        if (!_isGliding) return 1f;
        float y = fwd.y;
        return y > 0f ? 1f : (Mathf.Abs(y) * 1.5f) + 1f;
    }

    private float CalculateFallingVelocity()
    {
        float grav = _gravity * (_fallingVelocity < 0 ? _fallGravityMultiplier : 1);
        float max = _isGliding ? _glideMaxFallSpeed : _maxFallSpeed;
        float factor = _isGliding ? _glideGravityFactor : 1;

        return Mathf.Max(_fallingVelocity - grav * factor * Time.deltaTime, -max);
    }
/// <summary>
/// Adjusts horizontal velocity based on pitch angle while gliding.
/// Diving increases speed, climbing slows you down.
/// </summary>
private float CalculateGlidingAcceleration(float currentVelocity)
{
    // Get local X rotation (convert from 0–360 to -180–180)
    float xAngle = transform.localEulerAngles.x;
    if (xAngle > 180f)
        xAngle -= 360f;

    // Normalize angle to ratio (-1 to 1)
    float angleRatio = Mathf.Clamp(xAngle / _maxYZRotationAngle, -1f, 1f);
    float newVelocity;

    if (angleRatio > 0f)
    {
        // Diving → accelerate
        newVelocity = currentVelocity + (angleRatio * _glideAcceleration * Time.deltaTime);
    }
    else
    {
        // Climbing → decelerate
        newVelocity = currentVelocity + (angleRatio * _glideDeceleration * Time.deltaTime);
    }

    return Mathf.Clamp(newVelocity, _minSpeed, _absoluteMaxSpeed);
}
    private void ApplyFinalMovement()
    {
        Vector3 moveAttempt = _currentVelocity * Time.deltaTime;
        Vector3 finalMove = _colliderUtil.CollideAndSlide(moveAttempt, transform.position, 1, GetRampMultiplier());
        if (_isGrounded) finalMove = SnapToGround(finalMove);

        transform.position += finalMove;
        _currentVelocity = finalMove / Time.deltaTime;
    }

    // ====== HELPERS ======
    private float ClampAngle(float current, float min, float max)
    {
        float dtAngle = Mathf.Abs(((min - max) + 180) % 360 - 180);
        float halfAngle = dtAngle * 0.5f;
        float midAngle = min + halfAngle;
        float offset = Mathf.Abs(Mathf.DeltaAngle(current, midAngle)) - halfAngle;

        if (offset > 0)
            current = Mathf.MoveTowardsAngle(current, midAngle, offset);

        return current;
    }

    private float GetRampMultiplier()
    {
        float dot = Vector3.Dot(Vector3.up, _currentNormal);
        return (dot < 0.95f && dot > 0.05f) ? _rampMagnitudeMultiplier : 1f;
    }

    private Vector3 SnapToGround(Vector3 move)
    {
        move.y = Mathf.Max(0, move.y);
        return move;
    }

    private void FreezeFrame()
    {
        _isFrozen = true;
        StartCoroutine(UnfreezeFrame());
    }

    private IEnumerator UnfreezeFrame()
    {
        yield return new WaitForSecondsRealtime(_freezeFrameDuration);
        _isFrozen = false;
    }

    // ====== PUBLIC ACCESSORS ======
    public bool IsGrounded => _isGrounded;
    public bool IsBroadlyGrounded => _isBroadlyGrounded;
    public bool IsGliding => _isGliding;
    public bool IsOnWater => _currentGroundTag == "Water";
    public float Speed => _velocityMagnitude;
    public Vector2 InputVector => _input;
    public int Tier => _currentTier;
    public Vector3 ForwardVector => _forwardVector;
}
