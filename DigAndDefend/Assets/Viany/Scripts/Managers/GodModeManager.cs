using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GodModeManager : MonoBehaviour
{
    [SerializeField] Canvas godModeCanvas;
    [SerializeField] TMP_InputField copperInput;
    [SerializeField] TMP_InputField ironInput;
    [SerializeField] Toggle unlockTowersToggle;
    [SerializeField] TMP_InputField currentWaveInput;
    [SerializeField] TMP_InputField totalWavesInput;
    [SerializeField] Slider speedSlider;
    [SerializeField] TextMeshProUGUI speedValueText;
    [SerializeField] ShopManager shopManager;
    [SerializeField] UIManager uiManager;

    bool isGodModeActive;
    bool gPressed;
    bool oPressed;
    bool dPressed;
    bool canToggle = true;

    void Awake()
    {
        if (FindObjectsByType<GodModeManager>(FindObjectsSortMode.None).Length > 1)
            Destroy(gameObject);
    }

    void Start()
    {
        if (godModeCanvas)
            godModeCanvas.enabled = false;
        if (speedSlider)
        {
            speedSlider.minValue = 1f;
            speedSlider.maxValue = 5f;
            speedSlider.wholeNumbers = true;
            speedSlider.value = Time.timeScale;
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);
        }
        if (copperInput)
            copperInput.onEndEdit.AddListener(OnCopperInputChanged);
        if (ironInput)
            ironInput.onEndEdit.AddListener(OnIronInputChanged);
        if (unlockTowersToggle)
            unlockTowersToggle.onValueChanged.AddListener(OnUnlockTowersChanged);
        if (currentWaveInput)
            currentWaveInput.onEndEdit.AddListener(OnCurrentWaveChanged);
        if (totalWavesInput)
            totalWavesInput.onEndEdit.AddListener(OnTotalWavesChanged);
        UpdateInputFields();
    }

    void Update()
    {
        gPressed = Input.GetKey(KeyCode.G);
        oPressed = Input.GetKey(KeyCode.O);
        dPressed = Input.GetKey(KeyCode.D);

        if (gPressed && oPressed && dPressed && canToggle)
        {
            ToggleGodMode();
            canToggle = false;
        }
        else if (!gPressed || !oPressed || !dPressed)
        {
            canToggle = true;
        }
    }

    void ToggleGodMode()
    {
        isGodModeActive = !isGodModeActive;
        if (godModeCanvas)
            godModeCanvas.enabled = isGodModeActive;
        if (isGodModeActive)
            UpdateInputFields();
    }

    void UpdateInputFields()
    {
        if (copperInput && ResourceManager.Instance)
            copperInput.text = ResourceManager.Instance.copper.ToString();
        if (ironInput && ResourceManager.Instance)
            ironInput.text = ResourceManager.Instance.iron.ToString();
        if (currentWaveInput && GameManager.Instance)
            currentWaveInput.text = GameManager.Instance.currentWave.ToString();
        if (totalWavesInput && GameManager.Instance)
            totalWavesInput.text = GameManager.Instance.totalWaves.ToString();
        if (speedSlider)
            speedSlider.value = Time.timeScale;
        if (speedValueText)
            speedValueText.text = $"{(int)Time.timeScale}";
    }

    void OnCopperInputChanged(string value)
    {
        if (int.TryParse(value, out int newCopper) && ResourceManager.Instance)
            ResourceManager.Instance.copper = Mathf.Max(0, newCopper);
        if (ResourceManager.Instance)
            ResourceManager.Instance.UpdateResourceUI();
        UpdateInputFields();
    }

    void OnIronInputChanged(string value)
    {
        if (int.TryParse(value, out int newIron) && ResourceManager.Instance)
            ResourceManager.Instance.iron = Mathf.Max(0, newIron);
        if (ResourceManager.Instance)
            ResourceManager.Instance.UpdateResourceUI();
        UpdateInputFields();
    }

    void OnUnlockTowersChanged(bool isOn)
    {
        if (shopManager)
        {
            for (int i = 0; i < shopManager.unlockWaves.Length; i++)
                shopManager.unlockWaves[i] = isOn ? 0 : (i + 1) * 5;
            shopManager.UpdateShopContent();
        }
    }

    void OnCurrentWaveChanged(string value)
    {
        if (int.TryParse(value, out int newWave) && GameManager.Instance)
        {
            GameManager.Instance.currentWave = Mathf.Clamp(newWave, 0, GameManager.Instance.totalWaves);
            GameManager.Instance.WaveComplete();
            if (shopManager)
                shopManager.UpdateShopContent();
        }
        UpdateInputFields();
    }

    void OnTotalWavesChanged(string value)
    {
        if (int.TryParse(value, out int newTotalWaves) && GameManager.Instance)
        {
            GameManager.Instance.totalWaves = Mathf.Max(1, newTotalWaves);
            GameManager.Instance.WaveComplete();
        }
        UpdateInputFields();
    }

    void OnSpeedChanged(float value)
    {
        Time.timeScale = Mathf.Clamp(value, 1f, 5f);
        if (uiManager)
            uiManager.time = (int)Time.timeScale;
        if (speedValueText)
            speedValueText.text = $"{(int)value}";
    }
}