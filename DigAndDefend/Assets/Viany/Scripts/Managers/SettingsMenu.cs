using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI; // For Slider and Toggle
using TMPro; // For TextMeshProUGUI

public class SettingsMenu : MonoBehaviour
{
    public Toggle runCustomClassToggle; // Toggle UI object to run your custom class
    public Slider volumeSlider; // Volume slider
    public TextMeshProUGUI volumeLabel; // TMP text for volume percentage
    public AudioMixer audioMixer;

    void Start()
    {
        // Check for unassigned components
        if (volumeSlider == null)
        {
            Debug.LogError("volumeSlider is not assigned in the Inspector!");
            return;
        }
        if (audioMixer == null)
        {
            Debug.LogError("audioMixer is not assigned in the Inspector!");
            return;
        }
        if (volumeLabel == null)
        {
            Debug.LogWarning("volumeLabel is not assigned; percentage display will be skipped.");
        }
        if (runCustomClassToggle == null)
        {
            Debug.LogWarning("runCustomClassToggle is not assigned; custom class toggle will be skipped.");
        }

        // Volume slider setup
        volumeSlider.minValue = 0;
        volumeSlider.maxValue = 100;
        volumeSlider.wholeNumbers = true; // Ensure whole numbers for snapping
        volumeSlider.interactable = true; // Ensure interactable

        // Initialize volume
        float currentVolume;
        if (audioMixer.GetFloat("MasterVolume", out currentVolume))
        {
            volumeSlider.value = Mathf.RoundToInt(DBToLinear(currentVolume) * 100); // Convert dB to 0-100
            Debug.Log($"Initial MasterVolume: {currentVolume}dB, Slider Value: {volumeSlider.value}");
        }
        else
        {
            volumeSlider.value = 100; // Default to 50%
            Debug.LogWarning("Could not get MasterVolume from AudioMixer; defaulting to 50.");
        }

        // Set up slider listener with snapping
        volumeSlider.onValueChanged.RemoveAllListeners(); // Clear existing listeners
        volumeSlider.onValueChanged.AddListener(SetVolume);
        UpdateVolumeLabel(volumeSlider.value); // Initial label update

        // Set up toggle listener
        if (runCustomClassToggle != null)
        {
            runCustomClassToggle.onValueChanged.AddListener(RunCustomClass);
            Debug.Log($"Initial Toggle State: {runCustomClassToggle.isOn}");
        }

        // Load saved settings
        LoadSettings();
    }

    // Convert dB (-80 to 0) to linear (0 to 1)
    private float DBToLinear(float db)
    {
        return Mathf.Pow(10, db / 20);
    }

    // Convert linear (0 to 1) to dB (-80 to 0)
    private float LinearToDB(float linear)
    {
        float db = linear > 0 ? Mathf.Log10(linear) * 20 : -80f;
        return Mathf.Round(db / 10) * 10; // Snap volume to 10 dB increments
    }

    void SetVolume(float value)
    {
        // Snap slider value to nearest multiple of 10
        float snappedSliderValue = Mathf.Round(value / 10) * 10;
        if (Mathf.Abs(volumeSlider.value - snappedSliderValue) > 0.01f) // Prevent recursive calls
        {
            volumeSlider.value = snappedSliderValue;
            Debug.Log($"Snapped Slider to: {snappedSliderValue}%");
        }

        // Update AudioMixer with snapped volume
        float linear = snappedSliderValue / 100; // Convert 0-100 to 0-1
        if (audioMixer != null)
        {
            float snappedDB = LinearToDB(linear); // Snap to -80, -70, -60, ..., 0 dB
            audioMixer.SetFloat("MasterVolume", snappedDB);
            Debug.Log($"Set Volume: {snappedSliderValue}% ({snappedDB}dB)");
        }

        // Update label
        UpdateVolumeLabel(snappedSliderValue);
    }

    void UpdateVolumeLabel(float value)
    {
        if (volumeLabel != null)
        {
            volumeLabel.text = $"{value}%";
            Debug.Log($"Updated volumeLabel to: {value}%");
        }
    }

    void RunCustomClass(bool isOn)
    {
        if (runCustomClassToggle != null)
        {
            if (isOn)
            {
                Debug.Log("runCustomClassToggle is on; replace this with your custom class logic.");
                // TODO: Add your custom class call here, e.g.:
                // YourCustomClass.Instance.DoSomething();
            }
            else
            {
                Debug.Log("runCustomClassToggle is off; custom class not run.");
            }
        }
    }

    public void SaveSettings()
    {
        if (volumeSlider != null)
        {
            PlayerPrefs.SetFloat("MasterVolume", volumeSlider.value);
            PlayerPrefs.Save();
            Debug.Log("Settings saved.");
        }
    }

    void LoadSettings()
    {
        if (PlayerPrefs.HasKey("MasterVolume") && volumeSlider != null)
        {
            float volume = PlayerPrefs.GetFloat("MasterVolume");
            volumeSlider.value = volume; // Snapping handled in SetVolume
            if (audioMixer != null)
            {
                audioMixer.SetFloat("MasterVolume", LinearToDB(volume / 100));
            }
            UpdateVolumeLabel(volume);
            Debug.Log($"Loaded Volume: {volume}%");
        }
    }
}