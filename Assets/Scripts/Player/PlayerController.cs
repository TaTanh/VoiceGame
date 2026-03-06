using UnityEngine;

/// <summary>
/// Handles all player movement:
///   • Horizontal   – A/D or Left/Right arrow keys
///   • Vertical     – Microphone volume drives upward force; gravity handles descent
///
/// Requires:  Rigidbody2D, BoxCollider2D, VoiceInput (all on same GameObject)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    // ─────────────────────────── Inspector Fields ────────────────────────────

    [Header("Horizontal Movement")]
    [Tooltip("Horizontal movement speed in world-units per second.")]
    [SerializeField] public float movementSpeed = 5f;

    [Header("Voice Flight")]
    [Tooltip("Scales normalised volume [0..1] into Newtons of upward force.\n" +
             "Higher = player rises faster for the same voice volume.")]
    [SerializeField] public float voiceForceMultiplier = 20f;

    [Tooltip("Volume must exceed this value before any upward force is applied.\n" +
             "Tune this to eliminate false input from ambient noise.")]
    [SerializeField] public float voiceThreshold = 0.05f;

    [Tooltip("Hard cap on upward velocity to prevent the player flying off-screen.")]
    [SerializeField] public float maxVerticalVelocity = 10f;

    // ─────────────────────────── Public Properties ───────────────────────────

    /// <summary>
    /// The upward force (N) applied during the last physics step.
    /// Exposed for the DebugUI.
    /// </summary>
    public float LastAppliedUpwardForce { get; private set; }

    // ─────────────────────────── Private State ───────────────────────────────

    private Rigidbody2D _rb;
    private VoiceInput  _voiceInput;

    // ─────────────────────────── Unity Callbacks ─────────────────────────────

    private void Awake()
    {
        _rb         = GetComponent<Rigidbody2D>();
        _voiceInput = GetComponent<VoiceInput>();

        if (_voiceInput == null)
            Debug.LogWarning("[PlayerController] VoiceInput not found on this GameObject – voice flight is disabled.");
    }

    private void Update()
    {
        // Keyboard input / sprite flipping happen in Update (frame-rate independent enough for input)
        HandleHorizontalMovement();
    }

    private void FixedUpdate()
    {
        // Force application must happen in FixedUpdate to interact correctly with Rigidbody2D
        ApplyVoiceFlight();
        ClampVerticalVelocity();
    }

    // ─────────────────────────── Private Methods ─────────────────────────────

    /// <summary>
    /// Reads horizontal axis input and sets velocity directly for responsive feel.
    /// Also flips the sprite to face the movement direction.
    /// </summary>
    private void HandleHorizontalMovement()
    {
        // GetAxisRaw gives -1/0/+1 without smoothing – snappier for a platformer
        float horizontal = Input.GetAxisRaw("Horizontal");

        // Preserve vertical velocity while overriding horizontal
        _rb.velocity = new Vector2(horizontal * movementSpeed, _rb.velocity.y);

        // Flip by adjusting localScale.x so child objects (e.g. FirePoint) flip too
        if (horizontal > 0.01f)
            transform.localScale = new Vector3(1f, 1f, 1f);
        else if (horizontal < -0.01f)
            transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    /// <summary>
    /// Reads the current microphone volume and applies an upward force proportional
    /// to the loudness. Gravity (set on the Rigidbody2D) handles descent naturally.
    /// </summary>
    private void ApplyVoiceFlight()
    {
        if (_voiceInput == null)
        {
            LastAppliedUpwardForce = 0f;
            return;
        }

        float volume = _voiceInput.CurrentVolume;

        if (volume > voiceThreshold)
        {
            // Louder voice → larger force → faster ascent
            float upwardForce = volume * voiceForceMultiplier;
            _rb.AddForce(Vector2.up * upwardForce, ForceMode2D.Force);
            LastAppliedUpwardForce = upwardForce;
        }
        else
        {
            // Silence: no upward force. Gravity continues to pull the player down.
            LastAppliedUpwardForce = 0f;
        }
    }

    /// <summary>
    /// Prevents the player from accelerating upward indefinitely when shouting.
    /// </summary>
    private void ClampVerticalVelocity()
    {
        if (_rb.velocity.y > maxVerticalVelocity)
            _rb.velocity = new Vector2(_rb.velocity.x, maxVerticalVelocity);
    }
}
