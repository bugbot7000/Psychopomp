using UnityEngine;

/// <summary>
/// Handles all player-related and ambient audio, including
/// switching between flying and ambient tracks, and playing one-shot sound effects.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField, Tooltip("Ambient background music source.")]
    private AudioSource _ambientAudio;

    [SerializeField, Tooltip("Acceleration or speed-related audio source.")]
    private AudioSource _accAudio;

    [SerializeField, Tooltip("Flying mode background music source.")]
    private AudioSource _flyMusic;

    [SerializeField, Tooltip("Main theme or idle background music source.")]
    private AudioSource _themeAudio;

    [SerializeField, Tooltip("Audio source for player-specific SFX (like barrier, jumps, etc.).")]
    private AudioSource _playerSfx;

    [Header("Audio Clips")]
    [SerializeField, Tooltip("Sound clip played when the player breaks the sound barrier.")]
    private AudioClip _soundBarrierClip;

    [Header("Player Reference")]
    [SerializeField, Tooltip("Reference to the player's Kinematic Character Controller.")]
    private KinematicCharacterController _kcc;

    [Header("Debug Info")]
    [SerializeField, Tooltip("Current player speed for debug visualization.")]
    private float _playerSpeed;

    // ====== UNITY LIFECYCLE ======
    private void Start()
    {
        if (_kcc == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
                _kcc = player.GetComponent<KinematicCharacterController>();
        }

        // Prepare flying music but keep it paused initially
        if (_flyMusic != null)
        {
            _flyMusic.Play();
            _flyMusic.Pause();
        }
    }

    private void Update()
    {
        if (_kcc != null)
            _playerSpeed = _kcc.Speed;
    }

    // ====== AUDIO EVENTS ======

    /// <summary>
    /// Plays acceleration-related sound (like a boost or speed-up).
    /// </summary>
    public void PlayShockAudio()
    {
        if (_accAudio != null && _accAudio.clip != null)
            _accAudio.PlayOneShot(_accAudio.clip);
    }

    /// <summary>
    /// Plays the sound barrier SFX once.
    /// </summary>
    public void PlaySoundBarrier()
    {
        if (_playerSfx != null && _soundBarrierClip != null)
            _playerSfx.PlayOneShot(_soundBarrierClip);
    }

    /// <summary>
    /// Switches from ambient/theme music to flying music.
    /// </summary>
    public void SwitchToFlyingMusic()
    {
        if (_ambientAudio != null) _ambientAudio.Pause();
        if (_themeAudio != null) _themeAudio.Pause();
        if (_flyMusic != null) _flyMusic.UnPause();
    }

    /// <summary>
    /// Switches back from flying music to ambient/theme music.
    /// </summary>
    public void SwitchToAmbientMusic()
    {
        if (_ambientAudio != null) _ambientAudio.UnPause();
        if (_themeAudio != null) _themeAudio.UnPause();
        if (_flyMusic != null) _flyMusic.Pause();
    }
}
