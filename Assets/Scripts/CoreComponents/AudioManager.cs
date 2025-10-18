using UnityEngine;

/// <summary>
/// Handles all in-game music and sound transitions based on gameplay events.
/// Demonstrates clear event-driven separation between logic and audio.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _ambientAudio;
    [SerializeField] private AudioSource _flyMusic;
    [SerializeField] private AudioSource _accAudio;
    [SerializeField] private AudioSource _playerSfx;
    [SerializeField] private AudioClip _soundBarrierClip;

    private void OnEnable()
    {
        GameEvents.OnJump += PlayJump;
        GameEvents.OnStartGlide += SwitchToFlyingMusic;
        GameEvents.OnStopGlide += SwitchToAmbientMusic;
        GameEvents.OnPlayShockwave += PlaySoundBarrier;
        GameEvents.OnPlaySplash += PlaySplash;
        GameEvents.OnStopSplash += StopSplash;
    }

    private void OnDisable()
    {
        GameEvents.OnJump -= PlayJump;
        GameEvents.OnStartGlide -= SwitchToFlyingMusic;
        GameEvents.OnStopGlide -= SwitchToAmbientMusic;
        GameEvents.OnPlayShockwave -= PlaySoundBarrier;
        GameEvents.OnPlaySplash -= PlaySplash;
        GameEvents.OnStopSplash -= StopSplash;
    }

    private void PlayJump() => _accAudio?.PlayOneShot(_accAudio.clip);
    private void PlaySoundBarrier() => _playerSfx?.PlayOneShot(_soundBarrierClip);

    private void PlaySplash() => _playerSfx?.Play();
    private void StopSplash() => _playerSfx?.Stop();

    private void SwitchToFlyingMusic()
    {
        _ambientAudio?.Pause();
        _flyMusic?.UnPause();
    }

    private void SwitchToAmbientMusic()
    {
        _ambientAudio?.UnPause();
        _flyMusic?.Pause();
    }
}
