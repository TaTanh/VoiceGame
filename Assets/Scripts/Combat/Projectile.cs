using UnityEngine;

/// <summary>
/// A fast-moving projectile that damages enemies on contact and self-destructs
/// when it hits anything (enemy, wall, platform) or after its lifetime expires.
///
/// Prefab setup:
///   • Rigidbody2D  – Gravity Scale = 0, Collision Detection = Continuous
///   • Collider2D   – Any shape, Is Trigger = TRUE
///   • Tag the prefab as "Projectile"
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    // ─────────────────────────── Inspector Fields ────────────────────────────

    [Header("Projectile Settings")]
    [Tooltip("Travel speed in world-units per second.")]
    [SerializeField] private float speed = 15f;

    [Tooltip("Seconds before the projectile auto-destroys if it hits nothing.")]
    [SerializeField] private float lifetime = 3f;

    [Tooltip("Damage dealt to any EnemyHealth component on contact.")]
    [SerializeField] private int damage = 1;

    // ─────────────────────────── Private State ───────────────────────────────

    private Rigidbody2D _rb;

    // ─────────────────────────── Unity Callbacks ─────────────────────────────

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        // Projectiles travel in a straight line – disable gravity
        _rb.gravityScale = 0f;
        // Use Continuous detection to avoid tunnelling through thin objects at speed
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore other projectiles to allow overlapping shots
        if (other.CompareTag("Projectile"))
            return;

        // Ignore the player who fired this projectile
        if (other.CompareTag("Player"))
            return;

        // If we hit an enemy, deal damage before destroying
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
            enemyHealth.TakeDamage(damage);

        // Destroy on contact with anything that is not ignored above
        Destroy(gameObject);
    }

    // ─────────────────────────── Public API ──────────────────────────────────

    /// <summary>
    /// Sets the projectile's velocity and starts the lifetime countdown.
    /// Must be called immediately after Instantiate() by the spawning system.
    /// </summary>
    /// <param name="direction">Normalised 2D direction vector.</param>
    public void Initialize(Vector2 direction)
    {
        _rb.velocity = direction.normalized * speed;
        // Schedule self-destruction in case no collision ever occurs
        Destroy(gameObject, lifetime);
    }
}
