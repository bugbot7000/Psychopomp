using UnityEngine;

/// <summary>
/// Manages all character-related visual effects, animation states, 
/// model tilt, sound triggers, and tier-based visual feedback.
/// </summary>
public class CharacterVFX : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField, Tooltip("Animator controlling the character animations.")]
    private Animator _animator;

    [SerializeField, Tooltip("Transform of the character's visible model, used for tilt effects.")]
    private Transform _model;

    [SerializeField, Tooltip("Reference to the character controller.")]
    private KinematicCharacterController _kcc;

    private WwiseSoundManager _wwiseSoundManager;

    [Header("General Settings")]
    [SerializeField, Tooltip("Maximum speed used to scale tilt and effects.")]
    private float _maxSpeed = 150f;

    // ====== STATE VARIABLES ======
    private bool _onWater;
    private bool _isGrounded;
    private bool _isBroadlyGrounded;
    private bool _isGliding;
    private float _speed;
    private float _previousSpeed;
    private Vector2 _input;
    private int _tier;

    // Animator pause handling
    private float _previousAnimatorSpeed;

    // ====== PARTICLE SYSTEMS ======
    [Header("Particle Systems")]
    [SerializeField, Tooltip("All attached particle systems for tier-based updates.")]
    private ParticleController[] _particleSystems;

    // ====== SHOCKWAVE EFFECT ======
    [Header("Shockwave Effect")]
    [SerializeField, Tooltip("Material controlling shockwave visuals.")]
    private Material _shockwave;

    [SerializeField, Tooltip("Speed threshold to trigger a shockwave burst.")]
    private float _speedToTriggerShock = 90f;

    [SerializeField, Tooltip("Duration of the shockwave effect.")]
    private float _shockDuration = 1f;

    private bool _isShocking;
    private float _shockStart = -5f;
    private float _shockEnd = 1.5f;
    private float _shockElapsed;
    private bool _shockSfxPlayed = true;

    // ====== MODEL TILT ======
    [Header("Model Tilt Settings")]
    [SerializeField, Tooltip("Maximum tilt angle of the model when turning.")]
    private float _maxTilt = 30f;

    [SerializeField, Tooltip("Tilt interpolation intensity.")]
    private float _tiltIntensity = 1f;

    [SerializeField, Tooltip("Multiplier affecting tilt velocity responsiveness.")]
    private float _tiltVelocityMultiplier = 1f;

    // ====== SOUND FLAGS ======
    private bool _isWindSoundPlaying;
    private bool _isSplashPlaying;

    // ====== UNITY LIFECYCLE ======
    private void Start()
    {
        if (_kcc == null)
            _kcc = GetComponent<KinematicCharacterController>();

        _wwiseSoundManager = FindObjectOfType<WwiseSoundManager>();
        UpdateTier(0);
    }

    private void LateUpdate()
    {
        if (_kcc == null) return;

        UpdateVariables();
        UpdateAnimatorState();
        UpdateModelTilt();
        UpdateShockwave();
    }

    // ====== VARIABLE SYNC ======
    private void UpdateVariables()
    {
        _previousSpeed = _speed;

        _isGrounded = _kcc.IsGrounded;
        _isBroadlyGrounded = _kcc.IsBroadlyGrounded;
        _speed = _kcc.Speed;
        _isGliding = _kcc.IsGliding;
        _onWater = _kcc.IsOnWater;
        _input = _kcc.InputVector;

        if (!_isGliding)
            StopGlide();
    }

    // ====== ANIMATOR SYNC ======
    private void UpdateAnimatorState()
    {
        if (_animator == null) return;

        _animator.SetBool("TouchingGround", _isBroadlyGrounded);
        _animator.SetFloat("Speed", _speed);
        _animator.SetBool("IsGliding", _isGliding);
    }

    // ====== MODEL VISUAL TILT ======
    private void UpdateModelTilt()
    {
        if (_model == null) return;

        float targetTilt = 0f;
        if (_speed > 80f)
            targetTilt = -_input.x * _maxTilt;

        float lerpSpeed = _tiltIntensity * Time.deltaTime;
        Quaternion targetRotation = Quaternion.Euler(_model.localEulerAngles.x, _model.localEulerAngles.y, targetTilt);
        _model.localRotation = Quaternion.Lerp(_model.localRotation, targetRotation, lerpSpeed);
    }

    // ====== TIER SYSTEM ======
    public void UpdateTier(int newTier)
    {
        if (_tier < newTier && newTier > 1)
            TriggerShockwave();

        _tier = newTier;

        if (_particleSystems == null) return;
        foreach (ParticleController pc in _particleSystems)
        {
            if (pc != null)
                pc.UpdateTier(_tier);
        }
    }

    // ====== GLIDING SOUND & FX ======
    public void StartGlide()
    {
        if (_wwiseSoundManager == null)
            return;

        _wwiseSoundManager.MusicStartGliding();

        if (!_isWindSoundPlaying)
        {
            _wwiseSoundManager.PlayWindSound(CalculatePitch(), CalculateLowPass(), CalculateHighPass());
            _isWindSoundPlaying = true;
        }
    }

    public void StopGlide()
    {
        if (_wwiseSoundManager == null)
            return;

        _wwiseSoundManager.MusicStopGliding();
        _isWindSoundPlaying = false;
    }

    // ====== SHOCKWAVE EFFECT ======
    private void TriggerShockwave()
    {
        if (_isShocking) return;

        _isShocking = true;
        _shockSfxPlayed = false;
        _shockElapsed = 0;
    }

    private void UpdateShockwave()
    {
        if (!_isShocking || _shockwave == null) return;

        _shockElapsed += Time.deltaTime;
        _shockwave.SetFloat("_Progress", Mathf.Lerp(_shockStart, _shockEnd, _shockElapsed / _shockDuration));

        if (!_shockSfxPlayed && (_shockElapsed / _shockDuration) > 0.7f)
        {
            _shockSfxPlayed = true;
            _wwiseSoundManager?.PlaySoundWave();
        }

        if (_shockElapsed > _shockDuration)
            _isShocking = false;
    }

    // ====== AUDIO PARAMS ======
    private float CalculatePitch() => 10f;
    private float CalculateHighPass() => 50f;
    private float CalculateLowPass() => 20f;

    // ====== SPLASH SOUNDS ======
    public void PlaySplashSound()
    {
        if (_onWater && !_isSplashPlaying && _wwiseSoundManager != null)
        {
            _wwiseSoundManager.PlaySplash();
            _isSplashPlaying = true;
        }
    }

    public void StopSplashSound()
    {
        if (!_onWater && _isSplashPlaying && _wwiseSoundManager != null)
        {
            _wwiseSoundManager.StopSplash();
            _isSplashPlaying = false;
        }
    }

    // ====== OTHER SOUNDS ======
    public void PlayFeather()
    {
        _wwiseSoundManager?.PlayRandomFeather();
    }

    public void PlayJump()
    {
        _wwiseSoundManager?.PlayJump();
    }

    // ====== ANIMATOR CONTROL ======
    public void Pause()
    {
        if (_animator == null) return;

        _previousAnimatorSpeed = _animator.speed;
        _animator.speed = 0f;
    }

    public void Unpause()
    {
        if (_animator == null) return;
        _animator.speed = _previousAnimatorSpeed;
    }
}
