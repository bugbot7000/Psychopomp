using UnityEngine;

/// <summary>
/// Handles tier-specific particle emission based on speed tiers.
/// Subscribes to tier change events from the GameEvents hub.
/// </summary>
public class ParticleController : MonoBehaviour
{
    [Header("Particle Configuration")]
    [SerializeField] private int _thisTier = 0;
    [SerializeField] private ParticleSystem _ripple;

    private void OnEnable() => GameEvents.OnTierChanged += HandleTier;
    private void OnDisable() => GameEvents.OnTierChanged -= HandleTier;

    /// <summary>
    /// Activates or stops emission based on whether the player's tier matches.
    /// </summary>
    private void HandleTier(int tier)
    {
        if (tier == _thisTier)
            _ripple?.Play();
        else
            _ripple?.Stop();
    }
}
