using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool isGameOver { get; private set; } = false;
    public int currentWave { get; private set; } = 0; // Starts at 0, will increment on wave start
    [SerializeField] public int totalWaves = 10;
    [SerializeField] private GameObject startButton;
    [SerializeField] private Sprite startSprite;
    [SerializeField] private Sprite activeWaveSprite;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI hintText; // Hint text in the panel
    [SerializeField] private Image towerImage; // Tower image in the panel
    [SerializeField] private GameObject hintPanel; // Reference to the panel GameObject
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private Sprite archerTowerSprite; // Assign in Inspector
    [SerializeField] private Sprite slowTowerSprite;   // Assign in Inspector
    [SerializeField] private Sprite fireTowerSprite;   // Assign in Inspector
    [SerializeField] private Sprite bombTowerSprite;   // Assign in Inspector
    [SerializeField] private Canvas gameOverCanvas; // Variable for game over Canvas
    [SerializeField] private Canvas winCanvas; // Variable for win Canvas
    [SerializeField] public Canvas mainMenuCanvas; // New canvas
    [SerializeField] private Canvas pauseMenuCanvas; // New canvas
    [SerializeField] private Canvas inGameMenuCanvas; // New canvas
    [SerializeField] private Canvas guideMenuCanvas; // New canvas
    [SerializeField] private Canvas settingsMenuCanvas; // New canvas (assuming SettingsMenu uses this)
    [SerializeField] private VictoryLoseTextManager victoryLoseTextManager;

    [SerializeField] private int playerHealth = 100;
    public bool isWaveActive { get; private set; } = false;
    private List<(Vector3, int)> placedTowers = new List<(Vector3, int)>();
    private enum GameState { Playing, GameOver, Win }
    private GameState currentState = GameState.Playing;
    private bool showHints = true; // Default to showing hints
    private bool firstWaveStarted = false; // Track if first wave has been manually started

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        isGameOver = false;
        currentWave = 0; // Wave 1 will start from here
        playerHealth = 100;
        isWaveActive = false;
        UpdateUI();
        EnableStartButton(true);
        if (startButton != null)
        {
            startButton.SetActive(true); // Ensure button is visible
        }
        if (hintPanel != null)
        {
            hintPanel.SetActive(false); // Hide panel initially
        }
        if (gameOverCanvas != null)
        {
            gameOverCanvas.enabled = false; // Disable game over Canvas initially
        }
        if (winCanvas != null)
        {
            winCanvas.enabled = false; // Disable win Canvas initially
        }
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.enabled = true; // Disable main menu Canvas initially
        }
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.enabled = false; // Disable pause menu Canvas initially
        }
        if (inGameMenuCanvas != null)
        {
            inGameMenuCanvas.enabled = false; // Disable in-game menu Canvas initially
        }
        if (guideMenuCanvas != null)
        {
            guideMenuCanvas.enabled = false; // Disable guide menu Canvas initially
        }
        if (settingsMenuCanvas != null)
        {
            settingsMenuCanvas.enabled = false; // Disable settings menu Canvas initially
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentState != GameState.Playing) return;

        playerHealth -= damage;
        if (playerHealth <= 0)
        {
            playerHealth = 0;
            SetGameOver();
        }
        UpdateUI();
    }

    private void SetGameOver()
    {
        currentState = GameState.GameOver;
        isGameOver = true;
        isWaveActive = false;
        Debug.Log("Game Over!");
        EnableStartButton(false);
        Time.timeScale = 0f;
        victoryLoseTextManager.DisplayLoseText();
        gameOverCanvas.enabled = true;
        if (shopManager != null)
        {
            shopManager.CloseShop(); // Ensure shop is closed
        }
        if (hintPanel != null)
        {
            hintPanel.SetActive(false); // Hide hint panel
        }
        if (winCanvas != null)
        {
            winCanvas.enabled = false; // Ensure win Canvas is off
        }
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.enabled = false; // Ensure main menu is off
        }
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.enabled = false; // Ensure pause menu is off
        }
        if (inGameMenuCanvas != null)
        {
            inGameMenuCanvas.enabled = false; // Ensure in-game menu is off
        }
        if (guideMenuCanvas != null)
        {
            guideMenuCanvas.enabled = false; // Ensure guide menu is off
        }
        if (settingsMenuCanvas != null)
        {
            settingsMenuCanvas.enabled = false; // Ensure settings menu is off
        }
    }

    public void SetWin()
    {
        currentState = GameState.Win;
        isWaveActive = false;
        Debug.Log("Victory!");
        EnableStartButton(false);
        Time.timeScale = 0f;
        victoryLoseTextManager.DisplayVictoryText();
        winCanvas.enabled = true;
        if (shopManager != null)
        {
            shopManager.CloseShop(); // Ensure shop is closed
        }
        if (hintPanel != null)
        {
            hintPanel.SetActive(false); // Hide hint panel
        }
        if (gameOverCanvas != null)
        {
            gameOverCanvas.enabled = false; // Ensure game over Canvas is off
        }
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.enabled = false; // Ensure main menu is off
        }
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.enabled = false; // Ensure pause menu is off
        }
        if (inGameMenuCanvas != null)
        {
            inGameMenuCanvas.enabled = false; // Ensure in-game menu is off
        }
        if (guideMenuCanvas != null)
        {
            guideMenuCanvas.enabled = false; // Ensure guide menu is off
        }
        if (settingsMenuCanvas != null)
        {
            settingsMenuCanvas.enabled = false; // Ensure settings menu is off
        }
    }

    public void EnableStartButton(bool enable)
    {
        if (startButton != null && currentState == GameState.Playing)
        {
            bool inGrassArea = shopManager != null && !shopManager.IsInCaveArea();
            startButton.SetActive(true); // Keep button visible
            Button buttonComponent = startButton.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.interactable = enable && !isWaveActive && inGrassArea && (!IsAutoWaveEnabled() || !firstWaveStarted);
            }
            UpdateStartButtonSprite();
        }
    }

    public void StartWave()
    {
        if (currentState != GameState.Playing || !isWaveActive && !isGameOver && startButton.activeSelf)
        {
            if (shopManager != null && shopManager.IsInCaveArea()) return;
            isWaveActive = true;
            if (currentWave == 0) firstWaveStarted = true; // Mark first wave as started
            currentWave++; // Increment currentWave to the wave being started (e.g., 1, then 2, etc.)
            if (hintPanel != null) hintPanel.SetActive(false); // Hide panel when wave starts
            WaveManager waveManager = FindFirstObjectByType<WaveManager>();
            if (waveManager != null)
            {
                waveManager.StartWave();
            }
            UpdateUI();
            if (shopManager != null)
            {
                shopManager.UpdateMineButton();
            }
        }
    }

    public void WaveComplete()
    {
        isWaveActive = false;
        UpdateUI();
        UpdateStartButtonSprite();
        if (shopManager != null)
        {
            shopManager.UpdateMineButton();
        }
        if (currentWave >= totalWaves)
        {
            SetWin();
        }
        if (IsAutoWaveEnabled() && currentWave < totalWaves)
        {
            StartWave(); // Start next wave immediately if auto wave is enabled
            UpdateUI(); // Ensure UI updates with new wave count
        }
    }

    public void AddTower(Vector3 position, int index)
    {
        if (currentState == GameState.Playing)
        {
            placedTowers.Add((position, index));
        }
    }

    public List<(Vector3, int)> GetTowers()
    {
        return new List<(Vector3, int)>(placedTowers);
    }

    private void UpdateStartButtonSprite()
    {
        if (startButton != null && startButton.activeSelf && currentState == GameState.Playing)
        {
            Image buttonImage = startButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = isWaveActive ? activeWaveSprite : startSprite;
            }
        }
    }

    private void UpdateUI()
    {
        if (waveText != null) waveText.text = "Wave: " + currentWave + "/" + totalWaves; // Always show wave count
        if (healthText != null) healthText.text = "Health: " + (isGameOver ? 0 : playerHealth);
        if (hintText != null && !isWaveActive && currentState == GameState.Playing && showHints) // Only show hints when inactive
        {
            bool showHint = false;
            if (currentWave == 2)
            {
                hintText.text = "Consider adding a Slow Tower for tougher waves!";
                if (towerImage != null) towerImage.sprite = slowTowerSprite;
                showHint = true;
            }
            else if (currentWave == 4)
            {
                hintText.text = "Use a Bomb Tower to clear groups!";
                if (towerImage != null) towerImage.sprite = bombTowerSprite;
                showHint = true;
            }
            else if (currentWave == 6)
            {
                hintText.text = "Add a Fire Tower to handle groups!";
                if (towerImage != null) towerImage.sprite = fireTowerSprite;
                showHint = true;
            }
            else
            {
                hintText.text = "";
                if (towerImage != null) towerImage.sprite = null;
            }
            if (hintPanel != null) hintPanel.SetActive(showHint); // Show/hide panel based on hint
        }
    }

    public void ToggleHints(bool enable)
    {
        showHints = enable;
        if (hintPanel != null)
        {
            hintPanel.SetActive(false); // Hide panel when hints are disabled
        }
        UpdateUI(); // Refresh UI to reflect new hint state
    }

    public bool IsAutoWaveEnabled()
    {
        return SettingsMenu.Instance != null && SettingsMenu.Instance.autoWaveToggle != null && SettingsMenu.Instance.autoWaveToggle.isOn;
    }

    // Updated method to check if any canvas is open
    public bool IsAnyCanvasOpen()
    {
        return (gameOverCanvas != null && gameOverCanvas.enabled) ||
               (winCanvas != null && winCanvas.enabled) ||
               (mainMenuCanvas != null && mainMenuCanvas.enabled) ||
               (pauseMenuCanvas != null && pauseMenuCanvas.enabled) ||
               (guideMenuCanvas != null && guideMenuCanvas.enabled) ||
               (settingsMenuCanvas != null && settingsMenuCanvas.enabled);
    }
}