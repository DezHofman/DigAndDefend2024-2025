using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI; // For Slider and Toggle
using TMPro; // For TextMeshProUGUI

public class SettingsMenu : MonoBehaviour
{
    public static SettingsMenu Instance { get; private set; }

    public Toggle runCustomClassToggle; // Toggle to run your custom class
    public Toggle autoWaveToggle; // Toggle for Auto Wave On or Off
    public Toggle ingameGuideToggle; // Toggle for Ingame Guide On or Off
    public Toggle ingameSpeedToggle; // Toggle for Ingame Speed 1/2
    public Slider volumeSlider; // Volume slider
    public TextMeshProUGUI volumeLabel; // TMP text for volume percentage
    public AudioMixer audioMixer;
    public UIManager uiManager; // Reference to the existing UIManager script
    public Canvas settingsCanvas; // Reference to the Settings canvas

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

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
        if (autoWaveToggle == null)
        {
            Debug.LogWarning("autoWaveToggle is not assigned; auto wave toggle will be skipped.");
        }
        if (ingameGuideToggle == null)
        {
            Debug.LogWarning("ingameGuideToggle is not assigned; ingame guide toggle will be skipped.");
        }
        if (ingameSpeedToggle == null)
        {
            Debug.LogWarning("ingameSpeedToggle is not assigned; ingame speed toggle will be skipped.");
        }
        if (uiManager == null)
        {
            Debug.LogWarning("uiManager is not assigned; ingame speed will not update.");
        }
        if (settingsCanvas == null)
        {
            Debug.LogWarning("settingsCanvas is not assigned; settings UI will not be managed.");
        }

        // Volume slider setup
        volumeSlider.minValue = 0;
        volumeSlider.maxValue = 100;
        volumeSlider.wholeNumbers = true; // Ensure whole numbers for snapping
        volumeSlider.interactable = true; // Ensure interactable

        // Initialize volume
        float initialVolume = 100f; // Default fallback
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            initialVolume = PlayerPrefs.GetFloat("MasterVolume");
            Debug.Log($"Loaded initial volume from PlayerPrefs: {initialVolume}%");
        }
        else
        {
            float currentVolume;
            if (audioMixer.GetFloat("MasterVolume", out currentVolume))
            {
                initialVolume = Mathf.RoundToInt(DBToLinear(currentVolume) * 100);
                Debug.Log($"Loaded initial volume from AudioMixer: {currentVolume}dB ({initialVolume}%)");
            }
            else
            {
                Debug.LogWarning("Could not get MasterVolume from AudioMixer; using default 50%.");
            }
        }
        volumeSlider.value = initialVolume;
        UpdateVolumeLabel(initialVolume);

        // Set up slider listener with snapping
        volumeSlider.onValueChanged.RemoveAllListeners(); // Clear existing listeners
        volumeSlider.onValueChanged.AddListener(SetVolume);

        // Set up toggle listeners
        if (runCustomClassToggle != null)
        {
            runCustomClassToggle.onValueChanged.AddListener(RunCustomClass);
            Debug.Log($"Initial runCustomClassToggle State: {runCustomClassToggle.isOn}");
        }
        if (autoWaveToggle != null)
        {
            autoWaveToggle.isOn = PlayerPrefs.GetInt("AutoWave", 0) == 1; // Default to false
            autoWaveToggle.onValueChanged.AddListener(RunAutoWave);
            Debug.Log($"Initial autoWaveToggle State: {autoWaveToggle.isOn}");
        }
        if (ingameGuideToggle != null)
        {
            ingameGuideToggle.isOn = PlayerPrefs.GetInt("IngameGuide", 1) == 1; // Default to true
            ingameGuideToggle.onValueChanged.AddListener(RunIngameGuide);
            Debug.Log($"Initial ingameGuideToggle State: {ingameGuideToggle.isOn}");
        }
        if (ingameSpeedToggle != null)
        {
            ingameSpeedToggle.isOn = PlayerPrefs.GetInt("IngameSpeed", 1) == 1; // Default to true
            ingameSpeedToggle.onValueChanged.AddListener(RunIngameSpeed);
            Debug.Log($"Initial ingameSpeedToggle State: {ingameSpeedToggle.isOn}");
        }

        // Load saved settings for toggles
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

        // Save volume automatically
        PlayerPrefs.SetFloat("MasterVolume", snappedSliderValue);
        PlayerPrefs.Save();
        Debug.Log("Volume saved to PlayerPrefs.");
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

    void RunAutoWave(bool isOn)
    {
        if (autoWaveToggle != null)
        {
            if (isOn)
            {
                Debug.Log("autoWaveToggle is on; enabling auto wave.");
                PlayerPrefs.SetInt("AutoWave", 1);
            }
            else
            {
                Debug.Log("autoWaveToggle is off; disabling auto wave.");
                PlayerPrefs.SetInt("AutoWave", 0);
            }
            PlayerPrefs.Save();
        }
    }

    void RunIngameGuide(bool isOn)
    {
        if (ingameGuideToggle != null)
        {
            if (isOn)
            {
                Debug.Log("ingameGuideToggle is on; enabling hints.");
                GameManager.Instance.ToggleHints(true);
                PlayerPrefs.SetInt("IngameGuide", 1);
            }
            else
            {
                Debug.Log("ingameGuideToggle is off; disabling hints.");
                GameManager.Instance.ToggleHints(false);
                PlayerPrefs.SetInt("IngameGuide", 0);
            }
            PlayerPrefs.Save();
        }
    }

    void RunIngameSpeed(bool isOn)
    {
        if (ingameSpeedToggle != null && uiManager != null)
        {
            if (isOn)
            {
                Debug.Log("ingameSpeedToggle is on; setting time to 1.");
                uiManager.time = 1; // Set to time 1 when toggle is on
                PlayerPrefs.SetInt("IngameSpeed", 1);
            }
            else
            {
                Debug.Log("ingameSpeedToggle is off; setting time to 2.");
                uiManager.time = 2; // Set to time 2 when toggle is off
                PlayerPrefs.SetInt("IngameSpeed", 0);
            }
            PlayerPrefs.Save();
        }
    }

    public void SaveSettings()
    {
        if (volumeSlider != null)
        {
            PlayerPrefs.SetFloat("MasterVolume", volumeSlider.value);
            PlayerPrefs.Save();
            Debug.Log("Settings saved manually.");
        }
    }

    void LoadSettings()
    {
        if (autoWaveToggle != null && PlayerPrefs.HasKey("AutoWave"))
        {
            autoWaveToggle.isOn = PlayerPrefs.GetInt("AutoWave") == 1;
        }
        if (ingameGuideToggle != null && PlayerPrefs.HasKey("IngameGuide"))
        {
            ingameGuideToggle.isOn = PlayerPrefs.GetInt("IngameGuide") == 1;
        }
        if (ingameSpeedToggle != null && PlayerPrefs.HasKey("IngameSpeed"))
        {
            ingameSpeedToggle.isOn = PlayerPrefs.GetInt("IngameSpeed") == 1;
            if (uiManager != null)
            {
                uiManager.time = ingameSpeedToggle.isOn ? 1 : 2; // Load initial time
            }
        }
    }
}