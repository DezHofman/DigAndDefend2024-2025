using UnityEngine;
using UnityEngine.UI; // For Slider and Toggle
using TMPro; // For TextMeshProUGUI

public class SettingsMenu : MonoBehaviour
{
    public static SettingsMenu Instance { get; private set; }

    public Toggle runCustomClassToggle; // Toggle to run your custom class
    public Toggle ingameGuideToggle; // Toggle for Ingame Guide On or Off
    public Toggle ingameSpeedToggle; // Toggle for Ingame Speed 1/2
    public Slider volumeSlider; // Volume slider
    public TextMeshProUGUI volumeLabel; // TMP text for volume percentage
    public UIManager uiManager; // Reference to the existing UIManager script
    public Canvas settingsCanvas; // Reference to the Settings canvas

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Ensure this is a root GameObject before applying DontDestroyOnLoad
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                // Detach and make it a root GameObject
                transform.SetParent(null, true);
                DontDestroyOnLoad(gameObject);
            }
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
        if (volumeLabel == null)
        {
            Debug.LogWarning("volumeLabel is not assigned; percentage display will be skipped.");
        }
        if (runCustomClassToggle == null)
        {
            Debug.LogWarning("runCustomClassToggle is not assigned; custom class toggle will be skipped.");
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
        volumeSlider.interactable = true;

        // Initialize volume
        float initialVolume = 100f; // Default fallback
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            initialVolume = PlayerPrefs.GetFloat("MasterVolume");
        }
        volumeSlider.value = initialVolume;
        UpdateVolumeLabel(initialVolume);

        // Set up slider listener
        volumeSlider.onValueChanged.RemoveAllListeners();
        volumeSlider.onValueChanged.AddListener(SetVolume);

        if (runCustomClassToggle != null)
        {
            runCustomClassToggle.onValueChanged.AddListener(RunCustomClass);
        }
        if (ingameGuideToggle != null)
        {
            ingameGuideToggle.isOn = PlayerPrefs.GetInt("IngameGuide", 1) == 1;
            ingameGuideToggle.onValueChanged.AddListener(RunIngameGuide);
        }
        if (ingameSpeedToggle != null)
        {
            ingameSpeedToggle.isOn = PlayerPrefs.GetInt("IngameSpeed", 1) == 1;
            ingameSpeedToggle.onValueChanged.AddListener(RunIngameSpeed);
        }

        LoadSettings();
    }

    void SetVolume(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVolume(value);
        }
        UpdateVolumeLabel(value);
    }

    void UpdateVolumeLabel(float value)
    {
        if (volumeLabel != null)
        {
            volumeLabel.text = $"{value}%";
        }
    }

    void RunCustomClass(bool isOn)
    {
        if (runCustomClassToggle != null)
        {
            if (isOn)
            {
                Debug.Log("runCustomClassToggle is on; replace this with your custom class logic.");
            }
            else
            {
                Debug.Log("runCustomClassToggle is off; custom class not run.");
            }
        }
    }

    void RunIngameGuide(bool isOn)
    {
        if (ingameGuideToggle != null)
        {
            if (isOn)
            {
                GameManager.Instance.ToggleHints(true);
                PlayerPrefs.SetInt("IngameGuide", 1);
            }
            else
            {
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
                uiManager.time = 1;
                PlayerPrefs.SetInt("IngameSpeed", 1);
            }
            else
            {
                uiManager.time = 2;
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
        }
        PlayerPrefs.Save();
    }

    void LoadSettings()
    {
        if (ingameGuideToggle != null && PlayerPrefs.HasKey("IngameGuide"))
        {
            ingameGuideToggle.isOn = PlayerPrefs.GetInt("IngameGuide") == 1;
        }
        if (ingameSpeedToggle != null && PlayerPrefs.HasKey("IngameSpeed"))
        {
            ingameSpeedToggle.isOn = PlayerPrefs.GetInt("IngameSpeed") == 1;
            if (uiManager != null)
            {
                uiManager.time = ingameSpeedToggle.isOn ? 1 : 2;
            }
        }
        if (volumeSlider != null && PlayerPrefs.HasKey("MasterVolume"))
        {
            volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume");
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetVolume(volumeSlider.value);
            }
        }
    }
}