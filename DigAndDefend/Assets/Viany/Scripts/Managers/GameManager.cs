using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool isGameOver { get; private set; }
    [SerializeField] public int currentWave;
    [SerializeField] public int totalWaves = 20;
    [SerializeField] private Button startButton;
    [SerializeField] private Sprite startSprite;
    [SerializeField] private Sprite activeWaveSprite;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private Image towerImage;
    [SerializeField] private GameObject hintPanel;
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private Sprite archerTowerSprite;
    [SerializeField] private Sprite slowTowerSprite;
    [SerializeField] private Sprite fireTowerSprite;
    [SerializeField] private Sprite bombTowerSprite;
    [SerializeField] private Canvas gameOverCanvas;
    [SerializeField] private Canvas winCanvas;
    [SerializeField] public Canvas mainMenuCanvas;
    [SerializeField] private Canvas pauseMenuCanvas;
    [SerializeField] private Canvas inGameMenuCanvas;
    [SerializeField] private Canvas guideMenuCanvas;
    [SerializeField] private Canvas settingsMenuCanvas;
    [SerializeField] private VictoryLoseTextManager victoryLoseTextManager;

    [SerializeField] private int playerHealth = 100;
    public bool isWaveActive { get; private set; }
    private List<(Vector3, int)> placedTowers = new List<(Vector3, int)>();
    private enum GameState { Playing, GameOver, Win }
    private GameState currentState = GameState.Playing;
    private bool showHints = true;
    public WaveManager waveManager;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        isGameOver = false;
        currentWave = 0;
        playerHealth = 100;
        isWaveActive = false;
        UpdateUI();
        EnableStartButton(true);
        startButton.gameObject.SetActive(true);
        hintPanel.SetActive(false);
        gameOverCanvas.enabled = false;
        winCanvas.enabled = false;
        mainMenuCanvas.enabled = true;
        pauseMenuCanvas.enabled = false;
        inGameMenuCanvas.enabled = false;
        guideMenuCanvas.enabled = false;
        settingsMenuCanvas.enabled = false;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump") && isWaveActive == false)
        {
            StartWave();
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
        victoryLoseTextManager.DisplayLoseText();
        gameOverCanvas.enabled = true;
        winCanvas.enabled = false;
        DisableCanvas();
    }

    public void SetWin()
    {
        currentState = GameState.Win;
        victoryLoseTextManager.DisplayVictoryText();
        winCanvas.enabled = true;
        gameOverCanvas.enabled = false;
        DisableCanvas();
    }

    public void DisableCanvas()
    {
        mainMenuCanvas.enabled = false;
        pauseMenuCanvas.enabled = false;
        inGameMenuCanvas.enabled = false;
        guideMenuCanvas.enabled = false;
        settingsMenuCanvas.enabled = false;
        isWaveActive = false;
        EnableStartButton(false);
        Time.timeScale = 0f;
        shopManager.CloseShop();
        hintPanel.SetActive(false);
    }

    public void EnableStartButton(bool enable)
    {
        if (currentState == GameState.Playing)
        {
            bool inGrassArea = !shopManager.IsInCaveArea();
            startButton.gameObject.SetActive(true);
            startButton.interactable = inGrassArea && !isWaveActive;
            UpdateStartButtonSprite();
        }
    }

    public void StartWave()
    {
        if (currentState != GameState.Playing || !isWaveActive && !isGameOver && startButton.gameObject.activeSelf)
        {
            if (shopManager.IsInCaveArea())
            {
                return;
            }
            isWaveActive = true;
            currentWave++;
            hintPanel.SetActive(false);
            waveManager.StartWave();
            UpdateUI();
            EnableStartButton(false);
            UpdateStartButtonSprite();
            shopManager.UpdateMineButton();
        }
    }

    public void WaveComplete()
    {
        isWaveActive = false;
        UpdateUI();
        EnableStartButton(true);
        UpdateStartButtonSprite();
        shopManager.UpdateMineButton();
        if (currentWave >= totalWaves)
        {
            SetWin();
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
        if (startButton.gameObject.activeSelf && currentState == GameState.Playing)
        {
            Image buttonImage = startButton.GetComponent<Image>();
            buttonImage.sprite = isWaveActive ? activeWaveSprite : startSprite;
        }
    }

    private void UpdateUI()
    {
        waveText.text = "Wave: " + currentWave + "/" + totalWaves;
        healthText.text = "Health: " + (isGameOver ? 0 : playerHealth);
        if (!isWaveActive && currentState == GameState.Playing && showHints)
        {
            bool showHint = false;
            if (currentWave == 2)
            {
                hintText.text = "Consider adding a Slow Tower for tougher waves!";
                towerImage.sprite = slowTowerSprite;
                showHint = true;
            }
            else if (currentWave == 4)
            {
                hintText.text = "Use a Bomb Tower to clear groups!";
                towerImage.sprite = bombTowerSprite;
                showHint = true;
            }
            else if (currentWave == 6)
            {
                hintText.text = "Add a Fire Tower to handle groups!";
                towerImage.sprite = fireTowerSprite;
                showHint = true;
            }
            else
            {
                hintText.text = "";
                towerImage.sprite = null;
            }
            hintPanel.SetActive(showHint);
        }
    }

    public void ToggleHints(bool enable)
    {
        showHints = enable;
        hintPanel.SetActive(false);
        UpdateUI();
    }

    public bool IsAnyCanvasOpen()
    {
        return gameOverCanvas.enabled || winCanvas.enabled || mainMenuCanvas.enabled || pauseMenuCanvas.enabled || guideMenuCanvas.enabled || settingsMenuCanvas.enabled;
    }
}