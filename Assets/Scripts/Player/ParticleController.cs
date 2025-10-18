using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Handles all environmental and motion-based VFX for the player.
/// Includes ripples, dust, wind, and tier-specific emissions.
/// </summary>
public class ParticleController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField, Tooltip("Reference to the player's character controller.")]
    private KinematicCharacterController _kcc;

    [Header("Water Effects")]
    [Tooltip("Ripple particle when walking on water.")]
    [SerializeField] private ParticleSystem _ripple;

    [Tooltip("Time interval between water ripples.")]
    [SerializeField] private float _rippleInterval = 0.5f;

    private float _rippleTimer;
    private bool _isGrounded;
    private bool _isGliding;
    private bool _onWater;
    private float _speed;

// ====== TIER SYSTEM ======
[Header("Tier Settings")]
[SerializeField, Tooltip("The tier this particle system responds to.")]
private int _thisTier = 0;


private int _currentTier = 0;
private bool _isEmitting = false;

/// <summary>
/// Updates the particle controller based on the character's current speed tier.
/// If the current tier matches this particle's tier, it activates emission.
/// </summary>
public void UpdateTier(int newTier)
{
    _currentTier = newTier;

    if (_thisTier == newTier)
        StartEmitting();
    else
        StopEmitting();
}

/// <summary>
/// Starts emission for this particle controller (based on tier match).
/// </summary>
private void StartEmitting()
{
    _isEmitting = true;
    if (_ripple != null && !_ripple.isPlaying)
        _ripple.Play();
}

/// <summary>
/// Stops emission for this particle controller.
/// </summary>
private void StopEmitting()
{
    _isEmitting = false;
    if (_ripple != null && _ripple.isPlaying)
        _ripple.Stop();
}

    private void LateUpdate()
    {
        if (_kcc == null) return;

        UpdateVariables();
        HandleParticles();
    }

    /// <summary>
    /// Syncs movement and state variables from the character controller.
    /// </summary>
    private void UpdateVariables()
    {
        _isGrounded = _kcc.IsGrounded;
        _isGliding = _kcc.IsGliding;
        _onWater = _kcc.IsOnWater;
        _speed = _kcc.Speed;
    }

/// <summary>
/// Decides which particle effects to trigger based on current state.
/// </summary>
private void HandleParticles()
{
    if (!_isEmitting) return;

    if (_onWater && _isGrounded)
        TriggerRipple();
}


    /// <summary>
    /// Plays ripple effect periodically while on water.
    /// </summary>
    private void TriggerRipple()
    {
        if (_ripple == null) return;

        _rippleTimer += Time.deltaTime;

        if (_rippleTimer >= _rippleInterval)
        {
            _rippleTimer = 0f;
            _ripple.Play();
        }
    }
}
