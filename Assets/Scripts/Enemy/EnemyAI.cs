using UnityEngine;

/// <summary>
/// Simple platformer enemy that patrols back and forth on a platform.
/// Uses raycasts to detect platform edges and walls, then reverses direction.
/// A flip cooldown prevents jitter when multiple conditions trigger in the same frame.
///
/// Prefab setup:
///   • Rigidbody2D  – Freeze Rotation Z = true
///   • BoxCollider2D (non-trigger, sized to match sprite)
///   • Set the "Ground" layer on all platform GameObjects
///   • Assign the groundLayer mask in the Inspector
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class EnemyAI : MonoBehaviour
{
    // ─────────────────────────── Inspector Fields ────────────────────────────

    [Header("Patrol")]
    [Tooltip("Horizontal movement speed in world-units per second.")]
    [SerializeField] private float patrolSpeed = 2f;

    [Header("Detection")]
    [Tooltip("LayerMask for ground / platform geometry used by edge and wall raycasts.\n" +
             "Create a \"Ground\" layer and assign it here.")]
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("How far ahead of the enemy the edge-detection ray is cast (world units).")]
    [SerializeField] private float edgeProbeOffset = 0.4f;

    [Tooltip("Minimum seconds between direction reversals. Prevents rapid jitter\n" +
             "when both edge and wall detect a boundary in the same frame.")]
    [SerializeField] private float flipCooldown = 0.4f;

    // ─────────────────────────── Private State ───────────────────────────────

    private Rigidbody2D _rb;
    private int         _moveDirection = 1;   // +1 = right, -1 = left
    private float       _lastFlipTime  = -999f;

    // ─────────────────────────── Unity Callbacks ─────────────────────────────

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        // Keep the enemy upright – important for raycasts originating from transform.position
        _rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        MoveHorizontally();
        DetectEdge();
        DetectWall();
    }

    // ─────────────────────────── Private Methods ─────────────────────────────

    /// <summary>Applies horizontal velocity each physics step.</summary>
    private void MoveHorizontally()
    {
        _rb.velocity = new Vector2(patrolSpeed * _moveDirection, _rb.velocity.y);
    }

    /// <summary>
    /// Casts a short ray downward from slightly in front of the enemy's feet.
    /// If no ground is found, the enemy is near a platform edge and should reverse.
    /// </summary>
    private void DetectEdge()
    {
        // Probe origin: one step ahead horizontally, just below the pivot point
        Vector2 probeOrigin = (Vector2)transform.position
                            + new Vector2(_moveDirection * edgeProbeOffset, -0.05f);

        RaycastHit2D hit = Physics2D.Raycast(probeOrigin, Vector2.down, 1.2f, groundLayer);

        if (hit.collider == null)   // No ground ahead → reverse before falling off
            TryFlip();
    }

    /// <summary>
    /// Casts a short ray horizontally in the movement direction.
    /// If a wall is found, the enemy reverses.
    /// </summary>
    private void DetectWall()
    {
        RaycastHit2D hit = Physics2D.Raycast(
            (Vector2)transform.position,
            new Vector2(_moveDirection, 0f),
            0.55f,
            groundLayer);

        if (hit.collider != null)   // Wall ahead → reverse
            TryFlip();
    }

    /// <summary>
    /// Reverses the patrol direction and flips the sprite,
    /// but only if the flip cooldown has elapsed to avoid jitter.
    /// </summary>
    private void TryFlip()
    {
        if (Time.time - _lastFlipTime < flipCooldown)
            return;

        _moveDirection  *= -1;
        _lastFlipTime    = Time.time;

        // Flip the sprite by mirroring localScale.x
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * _moveDirection;
        transform.localScale = scale;
    }

    // ─────────────────────────── Gizmos (Editor Only) ────────────────────────

    private void OnDrawGizmosSelected()
    {
        // Edge detection ray (yellow)
        Gizmos.color = Color.yellow;
        Vector2 edgeOrigin = (Vector2)transform.position
                           + new Vector2(_moveDirection * edgeProbeOffset, -0.05f);
        Gizmos.DrawLine(edgeOrigin, edgeOrigin + Vector2.down * 1.2f);

        // Wall detection ray (red)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            transform.position,
            (Vector2)transform.position + new Vector2(_moveDirection * 0.55f, 0f));
    }
}
