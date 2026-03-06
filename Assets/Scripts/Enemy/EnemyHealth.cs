using UnityEngine;

/// <summary>
/// Tracks an enemy's health and handles its death.
///
/// Attach to: Enemy prefab (alongside EnemyAI).
/// Projectiles call TakeDamage() via OnTriggerEnter2D.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    // ─────────────────────────── Inspector Fields ────────────────────────────

    [Header("Health")]
    [Tooltip("Total hit points at spawn.")]
    [SerializeField] private int maxHealth = 3;

    // ─────────────────────────── Private State ───────────────────────────────

    private int _currentHealth;

    // ─────────────────────────── Unity Callbacks ─────────────────────────────

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    // ─────────────────────────── Public API ──────────────────────────────────

    /// <summary>
    /// Reduces health by <paramref name="amount"/>.
    /// Triggers death when health reaches zero or below.
    /// </summary>
    /// <param name="amount">Positive integer representing damage dealt.</param>
    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        _currentHealth -= amount;
        Debug.Log($"[EnemyHealth] {gameObject.name} hit for {amount}. HP: {_currentHealth}/{maxHealth}");

        if (_currentHealth <= 0)
            Die();
    }

    // ─────────────────────────── Private Methods ─────────────────────────────

    /// <summary>Plays death effects (extend here) and removes the enemy from the scene.</summary>
    private void Die()
    {
        Debug.Log($"[EnemyHealth] {gameObject.name} destroyed.");
        // TODO: spawn death VFX / sound, award score, etc.
        Destroy(gameObject);
    }
}
