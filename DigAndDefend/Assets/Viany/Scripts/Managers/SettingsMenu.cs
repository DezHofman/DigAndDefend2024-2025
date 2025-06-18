using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    public static SettingsMenu Instance { get; private set; }

    public Toggle runCustomClassToggle;
    public Toggle ingameGuideToggle;
    public Toggle ingameSpeedToggle;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeLabel;
    public UIManager uiManager;
    public Canvas settingsCanvas;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
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
        if (volumeSlider == null)
        {
            return;
        }

        volumeSlider.minValue = 0;
        volumeSlider.maxValue = 100;
        volumeSlider.wholeNumbers = true;
        volumeSlider.interactable = true;

        float initialVolume = 100f;
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            initialVolume = PlayerPrefs.GetFloat("MasterVolume");
        }
        volumeSlider.value = initialVolume;
        UpdateVolumeLabel(initialVolume);

        volumeSlider.onValueChanged.RemoveAllListeners();
        volumeSlider.onValueChanged.AddListener(SetVolume);
        
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