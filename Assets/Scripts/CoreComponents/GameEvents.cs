using System;
using UnityEngine;

/// <summary>
/// Centralized hub for all gameplay events. 
/// KinematicCharacterController, CharacterVFX, AudioManager, and CameraController.
/// </summary>
public static class GameEvents
{
    // ====== MOVEMENT EVENTS ======
    public static event Action OnJump;
    public static event Action OnLand;
    public static event Action OnStartGlide;
    public static event Action OnStopGlide;
    public static event Action<int> OnTierChanged;

    // ====== AUDIO EVENTS ======
    public static event Action OnPlaySplash;
    public static event Action OnStopSplash;
    public static event Action OnPlayShockwave;

    // ====== STATE EVENTS ======
    public static event Action OnPause;
    public static event Action OnUnpause;

    // ====== INVOKE HELPERS ======
    public static void Jump() => OnJump?.Invoke();
    public static void Land() => OnLand?.Invoke();
    public static void StartGlide() => OnStartGlide?.Invoke();
    public static void StopGlide() => OnStopGlide?.Invoke();
    public static void TierChanged(int tier) => OnTierChanged?.Invoke(tier);

    public static void PlaySplash() => OnPlaySplash?.Invoke();
    public static void StopSplash() => OnStopSplash?.Invoke();
    public static void PlayShockwave() => OnPlayShockwave?.Invoke();

    public static void Pause() => OnPause?.Invoke();
    public static void Unpause() => OnUnpause?.Invoke();
}
