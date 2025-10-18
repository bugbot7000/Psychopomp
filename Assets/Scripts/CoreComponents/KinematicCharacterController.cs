using System.Collections;
using UnityEngine;

/// <summary>
/// Handles all core character motion logic (input, jumping, gliding, gravity, coyote time)
/// and broadcasts player state changes through the GameEvents system.
/// </summary>
[RequireComponent(typeof(ColliderUtil))]
public class KinematicCharacterController : MonoBehaviour
{
    // ===== MOVEMENT CONFIG =====
    [Header("Movement Settings")]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 40f;
    [SerializeField] private float maxSpeed = 100f;

    // ===== GRAVITY / JUMP CONFIG =====
    [Header("Jump Settings")]
    [SerializeField] private float gravity = 10f;
    [SerializeField] private float jumpVelocity = 10f;
    [SerializeField] private int maxJumpCount = 2;
    [SerializeField] private float coyoteTime = 0.2f;       // Jump leniency after leaving ground
    [SerializeField] private float jumpBufferTime = 0.1f;   // Jump input buffering window

    // ===== STATE VARIABLES =====
    private Vector2 _input;
    private Vector3 _velocity;
    private bool _isGrounded;
    private bool _wasGrounded;
    private bool _isGliding;
    private float _speed;
    private float _fallingVelocity;
    private float _currentCoyote;
    private float _currentJumpBuffer;
    private int _jumpCount;

    // Public read-only accessors for other systems
    public bool IsGrounded => _isGrounded;
    public bool IsGliding => _isGliding;
    public float Speed => _speed;
    public Vector3 ForwardVector => transform.forward;

    // Cached component reference
    private ColliderUtil _colUtil;

    private void Start()
    {
        _colUtil = GetComponent<ColliderUtil>();
    }

    private void Update()
    {
        HandleInput();
        HandleGroundChecks();
        HandleJumping();
        HandleGliding();
        HandleMovement();
    }

    // ----- INPUT -----
    private void HandleInput()
    {
        _input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    // ----- GROUND DETECTION -----
    private void HandleGroundChecks()
    {
        _wasGrounded = _isGrounded;
        _isGrounded = _colUtil.IsGroundedCast(transform.position, out _, out _);

        // Landing detection
        if (_isGrounded && !_wasGrounded)
        {
            GameEvents.Land();
            _jumpCount = 0;
            _currentCoyote = coyoteTime;
        }
        else if (!_isGrounded)
        {
            _currentCoyote -= Time.deltaTime;
        }
    }

    // ----- JUMP LOGIC -----
    private void HandleJumping()
    {
        // Store jump input in buffer to allow late jump execution
        if (Input.GetButtonDown("Jump"))
            _currentJumpBuffer = jumpBufferTime;
        else
            _currentJumpBuffer -= Time.deltaTime;

        bool canJump = _currentCoyote > 0f && _currentJumpBuffer > 0f;

        if (canJump)
        {
            _jumpCount++;
            _fallingVelocity = jumpVelocity;
            _currentJumpBuffer = 0;
            _currentCoyote = 0;
            GameEvents.Jump(); // Broadcast jump event
        }
    }

    // ----- GLIDE LOGIC -----
    private void HandleGliding()
    {
        bool glideInput = Input.GetButton("Glide");

        if (!_isGrounded && glideInput && !_isGliding)
        {
            _isGliding = true;
            GameEvents.StartGlide();
        }
        else if ((!glideInput || _isGrounded) && _isGliding)
        {
            _isGliding = false;
            GameEvents.StopGlide();
        }
    }

    // ----- MOVEMENT INTEGRATION -----
    private void HandleMovement()
    {
        // Simple velocity integration
        _speed += (_input.y > 0 ? acceleration : -deceleration) * Time.deltaTime;
        _speed = Mathf.Clamp(_speed, 0, maxSpeed);

        if (!_isGrounded)
            _fallingVelocity -= gravity * Time.deltaTime;

        Vector3 move = transform.forward * _speed * Time.deltaTime;
        move.y += _fallingVelocity * Time.deltaTime;
        transform.position += move;
    }
}
