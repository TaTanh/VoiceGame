using UnityEngine;

/// <summary>
/// Captures audio from the default microphone device and exposes a normalized
/// volume level [0..1] that other systems (e.g. PlayerController) can read.
///
/// Usage: Attach to the Player GameObject.
/// </summary>
public class VoiceInput : MonoBehaviour
{
    // ─────────────────────────── Inspector Fields ────────────────────────────

    [Header("Microphone Settings")]
    [Tooltip("Number of most-recent audio samples analysed per frame.\n" +
             "Power-of-two values are most efficient (64, 128, 256…).")]
    [SerializeField] private int sampleWindowSize = 128;

    [Tooltip("Duration of the rolling microphone recording buffer in seconds.")]
    [SerializeField] private int recordingBufferSeconds = 1;

    [Header("Volume Calibration")]
    [Tooltip("Multiplier applied to the raw RMS amplitude before clamping to [0,1].\n" +
             "Increase this value if your microphone is quiet.")]
    [SerializeField] private float sensitivityMultiplier = 100f;

    [Tooltip("RMS values at or below this threshold are treated as silence, " +
             "preventing ambient noise from generating lift.")]
    [SerializeField] private float noiseFloor = 0.005f;

    // ─────────────────────────── Public Properties ───────────────────────────

    /// <summary>Normalised microphone amplitude, clamped to [0, 1].</summary>
    public float CurrentVolume { get; private set; }

    // ─────────────────────────── Private State ───────────────────────────────

    private AudioClip _microphoneClip;
    private string    _microphoneDevice;
    private bool      _isInitialized;

    // ─────────────────────────── Unity Callbacks ─────────────────────────────

    private void Start()
    {
        InitializeMicrophone();
    }

    private void Update()
    {
        // Recompute volume every frame so consumers always have the latest value
        CurrentVolume = _isInitialized ? ComputeNormalizedVolume() : 0f;
    }

    private void OnDestroy()
    {
        // Always stop the microphone cleanly to release the OS resource
        if (_isInitialized && Microphone.IsRecording(_microphoneDevice))
            Microphone.End(_microphoneDevice);
    }

    // ─────────────────────────── Private Methods ─────────────────────────────

    /// <summary>
    /// Starts recording from the first available microphone device.
    /// Logs a warning and disables itself gracefully when no device is found.
    /// </summary>
    private void InitializeMicrophone()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("[VoiceInput] No microphone device detected – voice flight is disabled.");
            return;
        }

        // Use the first device reported by the OS (index 0 = system default on most platforms)
        _microphoneDevice = Microphone.devices[0];

        // Loop = true so that the clip rolls over endlessly (no gap in data)
        _microphoneClip = Microphone.Start(
            _microphoneDevice,
            loop: true,
            lengthSec: recordingBufferSeconds,
            frequency: AudioSettings.outputSampleRate);

        _isInitialized = true;
        Debug.Log($"[VoiceInput] Microphone started: \"{_microphoneDevice}\"");
    }

    /// <summary>
    /// Reads the most recent <see cref="sampleWindowSize"/> samples from the
    /// microphone clip and returns their RMS amplitude normalised to [0, 1].
    /// </summary>
    private float ComputeNormalizedVolume()
    {
        // The microphone write-head position in samples
        int micPosition = Microphone.GetPosition(_microphoneDevice);

        // Wait until the buffer has been filled at least once before reading
        if (micPosition < sampleWindowSize)
            return 0f;

        // Copy the most recent samples into a temporary array
        float[] samples = new float[sampleWindowSize];
        _microphoneClip.GetData(samples, micPosition - sampleWindowSize);

        // Root Mean Square: converts signed oscillating signal to positive energy level
        float sumSquares = 0f;
        for (int i = 0; i < samples.Length; i++)
            sumSquares += samples[i] * samples[i];

        float rms = Mathf.Sqrt(sumSquares / samples.Length);

        // Reject values that fall within the ambient noise floor
        if (rms < noiseFloor)
            return 0f;

        // Scale and clamp to produce a clean normalised value
        return Mathf.Clamp01(rms * sensitivityMultiplier);
    }
}
