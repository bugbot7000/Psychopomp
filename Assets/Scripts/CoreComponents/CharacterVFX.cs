using UnityEngine;

/// <summary>
/// Responds to gameplay events (jump, glide, tier change)
/// and updates animation and visual feedback accordingly.
/// </summary>
public class CharacterVFX : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private Material _shockwave;

    [Header("Shockwave Settings")]
    [SerializeField] private float _shockDuration = 1f;

    private bool _isShocking;
    private float _shockElapsed;

    private void OnEnable()
    {
        // Subscribe to GameEvents
        GameEvents.OnJump += PlayJump;
        GameEvents.OnStartGlide += StartGlide;
        GameEvents.OnStopGlide += StopGlide;
        GameEvents.OnTierChanged += UpdateTier;
        GameEvents.OnPlayShockwave += TriggerShock;
    }

    private void OnDisable()
    {
        // Clean up subscriptions
        GameEvents.OnJump -= PlayJump;
        GameEvents.OnStartGlide -= StartGlide;
        GameEvents.OnStopGlide -= StopGlide;
        GameEvents.OnTierChanged -= UpdateTier;
        GameEvents.OnPlayShockwave -= TriggerShock;
    }

    private void LateUpdate()
    {
        // Update shockwave animation over time
        if (_isShocking)
        {
            _shockElapsed += Time.deltaTime;
            _shockwave.SetFloat("_Progress", _shockElapsed / _shockDuration);
            if (_shockElapsed > _shockDuration) _isShocking = false;
        }
    }

    private void PlayJump() => _animator.SetTrigger("Jump");
    private void StartGlide() => _animator.SetBool("IsGliding", true);
    private void StopGlide() => _animator.SetBool("IsGliding", false);
    private void UpdateTier(int tier) => _animator.SetFloat("Speed", tier * 10);
    private void TriggerShock() { _isShocking = true; _shockElapsed = 0; }
}
