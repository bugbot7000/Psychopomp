using System.Collections;
using UnityEngine;
using Cinemachine;

/// <summary>
/// Controls all Cinemachine virtual cameras for player states (grounded, flying, gliding, etc.).
/// Handles camera switching, glide shake, and pause/unpause transitions.
/// </summary>
public class CameraController : MonoBehaviour
{
    // ====== CAMERA REFERENCES ======
    [Header("Cameras")]
    [SerializeField, Tooltip("All active virtual cameras used by the player.")]
    private CinemachineVirtualCamera[] _cameras;

    [SerializeField, Tooltip("Close-range cameras used during high-speed movement.")]
    private CinemachineVirtualCamera[] _nearCameras;

    [SerializeField, Tooltip("Far-range cameras activated on high-speed tiers.")]
    private CinemachineVirtualCamera[] _farCameras;

    [SerializeField, Tooltip("Cameras used when gliding.")]
    private CinemachineVirtualCamera[] _glideCameras;

    [SerializeField, Tooltip("The camera that is active when the scene starts.")]
    private CinemachineVirtualCamera _initialCamera;

    private CinemachineVirtualCamera _currentCamera;

    // ====== CAMERA BEHAVIOR SETTINGS ======
    [Header("Camera Settings")]
    [SerializeField, Tooltip("Time delay before switching back to near camera after a far-tier event.")]
    private float _switchToNearDelay = 1f;

    [SerializeField, Tooltip("Camera shake frequency curve during gliding.")]
    private AnimationCurve _glideNoiseFreqCurve;

    [SerializeField, Tooltip("Camera shake amplitude curve during gliding.")]
    private AnimationCurve _glideNoiseAmpCurve;

    // ====== CHARACTER REFERENCE ======
    [Header("Player Reference")]
    [SerializeField, Tooltip("Reference to the player's character controller.")]
    private KinematicCharacterController _kcc;

    private bool _isGliding;
    private bool _isGrounded;
    private float _speed;
    private int _currentTier;
    private Vector3 _forwardVector;

    // ====== PAUSE VARIABLES ======
    private bool _isPaused;
    private float _previousAmplitude;

    // ====== UNITY LIFECYCLE ======
    private void Start()
    {
        _currentCamera = _initialCamera;
        ActivateCamera(_currentCamera);
    }

    private void LateUpdate()
    {
        if (_kcc == null) return;

        UpdateVariables();

        if (_isGliding)
            UpdateGlidingCamera();
    }

    // ====== CAMERA TIER SYSTEM ======
    /// <summary>
    /// Updates the camera tier based on the player's current speed level.
    /// </summary>
    public void UpdateTier(int newTier)
    {
        if (_currentTier < newTier && newTier > 1)
        {
            SwitchToCamera(_farCameras[newTier]);
            StartCoroutine(TimedSwitchToNear());
        }

        _currentTier = newTier;
    }

    /// <summary>
    /// Switches the active virtual camera.
    /// </summary>
    private void SwitchToCamera(CinemachineVirtualCamera cam)
    {
        if (cam == null || cam == _currentCamera) return;

        _currentCamera = cam;
        ActivateCamera(_currentCamera);
    }

    /// <summary>
    /// Activates one camera and lowers priority of all others.
    /// </summary>
    private void ActivateCamera(CinemachineVirtualCamera cam)
    {
        if (_cameras == null || _cameras.Length == 0) return;

        foreach (var virtualCam in _cameras)
            virtualCam.Priority = (virtualCam == cam) ? 20 : 10;
    }

    // ====== VARIABLE SYNC ======
    /// <summary>
    /// Pulls state data from the character controller.
    /// </summary>
    private void UpdateVariables()
    {
        _isGrounded = _kcc.IsGrounded;
        _speed = _kcc.Speed;
        _forwardVector = _kcc.ForwardVector;

        bool newGlideState = _kcc.IsGliding;

        if (newGlideState != _isGliding)
        {
            if (newGlideState)
                StartGlide();
            else
                StopGlide();
        }

        _isGliding = newGlideState;
    }

    // ====== CAMERA SWITCH TIMERS ======
    private IEnumerator TimedSwitchToNear()
    {
        yield return new WaitForSeconds(_switchToNearDelay);
        if (_nearCameras != null && _nearCameras.Length > _currentTier)
            SwitchToCamera(_nearCameras[_currentTier]);
    }

    private void StartGlide()
    {
        if (_glideCameras != null && _glideCameras.Length > _currentTier)
            SwitchToCamera(_glideCameras[_currentTier]);
    }

    private void StopGlide()
    {
        if (_nearCameras != null && _nearCameras.Length > _currentTier)
            SwitchToCamera(_nearCameras[_currentTier]);
    }

    // ====== GLIDE CAMERA EFFECTS ======
    /// <summary>
    /// Dynamically adjusts camera shake based on glide angle.
    /// </summary>
    private void UpdateGlidingCamera()
    {
        if (_glideCameras == null || _glideCameras.Length <= _currentTier) return;

        float yAxis = _forwardVector.y;
        float frequency = 0f;
        float amplitude = 0f;

        if (yAxis < 0f)
        {
            frequency = _glideNoiseFreqCurve.Evaluate(Mathf.Abs(yAxis));
            amplitude = _glideNoiseAmpCurve.Evaluate(Mathf.Abs(yAxis));
        }

        UpdateCameraNoise(_glideCameras[_currentTier], frequency, amplitude);
    }

    /// <summary>
    /// Updates the Cinemachine noise component for gliding effects.
    /// </summary>
    private void UpdateCameraNoise(CinemachineVirtualCamera cam, float frequency, float amplitude)
    {
        if (cam == null) return;

        if (_isPaused)
        {
            frequency = 0f;
            amplitude = 0f;
        }

        var noise = cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (noise == null) return;

        noise.m_FrequencyGain = frequency;
        noise.m_AmplitudeGain = amplitude;
    }

    // ====== PAUSE & UNPAUSE ======
    public void Pause()
    {
        _isPaused = true;

        if (_currentCamera == null) return;

        var noise = _currentCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (noise == null) return;

        _previousAmplitude = noise.m_AmplitudeGain;
        noise.m_AmplitudeGain = 0f;
    }

    public void Unpause()
    {
        _isPaused = false;

        if (_currentCamera == null) return;

        var noise = _currentCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (noise == null) return;

        noise.m_AmplitudeGain = _previousAmplitude;
    }
}