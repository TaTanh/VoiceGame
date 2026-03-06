using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Real-time debug overlay that displays microphone volume and upward force.
///
/// Setup:
///   1. Create a Canvas (Screen Space – Overlay).
///   2. Add an empty child Panel and attach this script to it.
///   3. Add two TMP_Text children and assign them to the fields below.
///   4. (Optional) Add a UI Image (Image Type = Filled, Fill Method = Horizontal)
///      and assign it to volumeBar for a visual bar.
///   5. Drag the Player GameObject into the playerController / voiceInput fields.
/// </summary>
public class DebugUI : MonoBehaviour
{
    // ─────────────────────────── Inspector Fields ────────────────────────────

    [Header("References – assign in Inspector")]
    [Tooltip("PlayerController component on the Player GameObject.")]
    [SerializeField] private PlayerController playerController;

    [Tooltip("VoiceInput component on the Player GameObject.")]
    [SerializeField] private VoiceInput voiceInput;

    [Header("UI Elements")]
    [Tooltip("TMP_Text that shows the current microphone volume.")]
    [SerializeField] private TMP_Text volumeLabel;

    [Tooltip("TMP_Text that shows the upward force applied last physics step.")]
    [SerializeField] private TMP_Text forceLabel;

    [Tooltip("(Optional) Filled Image used as a visual volume bar.")]
    [SerializeField] private Image volumeBar;

    // ─────────────────────────── Unity Callbacks ─────────────────────────────

    private void Update()
    {
        // Guard: do nothing if references are missing
        if (voiceInput == null || playerController == null)
            return;

        float volume      = voiceInput.CurrentVolume;
        float upwardForce = playerController.LastAppliedUpwardForce;

        // ── Text labels ──────────────────────────────────────────────────────

        if (volumeLabel != null)
            volumeLabel.text = $"Mic Volume:    {volume:F3}";

        if (forceLabel != null)
            forceLabel.text = $"Upward Force:  {upwardForce:F2} N";

        // ── Visual volume bar (optional) ─────────────────────────────────────
        // fillAmount maps 0..1 directly to the normalised volume
        if (volumeBar != null)
            volumeBar.fillAmount = volume;
    }
}
