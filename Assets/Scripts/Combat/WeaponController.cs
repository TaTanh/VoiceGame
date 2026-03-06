using UnityEngine;

/// <summary>
/// Handles player shooting. Fires a Projectile prefab from a designated
/// fire point every time the player holds left mouse button (respecting fire rate).
///
/// Attach to: Player GameObject
/// Requires:  Assign projectilePrefab and firePoint in the Inspector.
/// </summary>
public class WeaponController : MonoBehaviour
{
    // ─────────────────────────── Inspector Fields ────────────────────────────

    [Header("Weapon Settings")]
    [Tooltip("Drag the Projectile prefab from the Project window here.")]
    [SerializeField] private GameObject projectilePrefab;

    [Tooltip("Empty child GameObject that marks the barrel tip / spawn position.\n" +
             "Create a child called \"FirePoint\" and assign it here.")]
    [SerializeField] private Transform firePoint;

    [Tooltip("Minimum seconds between consecutive shots (1 / fire rate).")]
    [SerializeField] private float fireRate = 0.25f;

    // ─────────────────────────── Private State ───────────────────────────────

    private float _nextFireTime;

    // ─────────────────────────── Unity Callbacks ─────────────────────────────

    private void Update()
    {
        // Hold to fire – respects fire-rate cooldown
        if (Input.GetMouseButton(0) && Time.time >= _nextFireTime)
        {
            Fire();
            _nextFireTime = Time.time + fireRate;
        }
    }

    // ─────────────────────────── Private Methods ─────────────────────────────

    /// <summary>
    /// Spawns a projectile at the fire point and sends it in the direction
    /// the player is currently facing (determined by localScale.x sign).
    /// </summary>
    private void Fire()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[WeaponController] Projectile prefab is not assigned.");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogWarning("[WeaponController] FirePoint transform is not assigned.");
            return;
        }

        // lossyScale accounts for any parent scaling; sign gives the facing direction
        float facingDirection = Mathf.Sign(transform.lossyScale.x);
        Vector2 shotDirection = new Vector2(facingDirection, 0f);

        // Instantiate at the fire point with no rotation (projectile handles its own velocity)
        GameObject projectileObj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
            projectile.Initialize(shotDirection);
        else
            Debug.LogWarning("[WeaponController] Instantiated object is missing a Projectile component.");
    }
}
