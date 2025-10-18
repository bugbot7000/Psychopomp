using System.Collections;
using UnityEngine;
using Cinemachine;

/// <summary>
/// Manages dynamic camera transitions for different player states (tier upgrades, glide).
/// Listens to GameEvents for camera switching.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Camera Sets")]
    [SerializeField] private CinemachineVirtualCamera[] _nearCameras;
    [SerializeField] private CinemachineVirtualCamera[] _farCameras;
    [SerializeField] private CinemachineVirtualCamera[] _glideCameras;
    [SerializeField] private float _switchToNearDelay = 1f;

    private int _currentTier;

    private void OnEnable()
    {
        GameEvents.OnStartGlide += StartGlide;
        GameEvents.OnStopGlide += StopGlide;
        GameEvents.OnTierChanged += UpdateTier;
    }

    private void OnDisable()
    {
        GameEvents.OnStartGlide -= StartGlide;
        GameEvents.OnStopGlide -= StopGlide;
        GameEvents.OnTierChanged -= UpdateTier;
    }

    private void UpdateTier(int tier)
    {
        _currentTier = tier;
        if (_farCameras != null && tier < _farCameras.Length)
        {
            ActivateCamera(_farCameras[tier]);
            StartCoroutine(SwitchBackToNear());
        }
    }

    private IEnumerator SwitchBackToNear()
    {
        yield return new WaitForSeconds(_switchToNearDelay);
        if (_nearCameras != null && _currentTier < _nearCameras.Length)
            ActivateCamera(_nearCameras[_currentTier]);
    }

    private void StartGlide()
    {
        if (_glideCameras != null && _currentTier < _glideCameras.Length)
            ActivateCamera(_glideCameras[_currentTier]);
    }

    private void StopGlide()
    {
        if (_nearCameras != null && _currentTier < _nearCameras.Length)
            ActivateCamera(_nearCameras[_currentTier]);
    }

    /// <summary>
    /// Sets the specified camera as active by priority.
    /// </summary>
    private void ActivateCamera(CinemachineVirtualCamera cam)
    {
        if (cam == null) return;
        foreach (var c in FindObjectsOfType<CinemachineVirtualCamera>())
            c.Priority = (c == cam) ? 20 : 10;
    }
}
