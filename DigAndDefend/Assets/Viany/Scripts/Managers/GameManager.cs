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

    private int playerHealth = 100;
    public bool isWaveActive { get; private set; } = false;
    private List<(Vector3, int)> placedTowers = new List<(Vector3, int)>();

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
            startButton.SetActive(true);
        }
        if (hintPanel != null)
        {
            hintPanel.SetActive(false); // Hide panel initially
        }
    }

    public void TakeDamage(int damage)
    {
        if (isGameOver) return;

        playerHealth -= damage;
        if (playerHealth <= 0)
        {
            playerHealth = 0;
            isGameOver = true;
            Debug.Log("Game Over!");
        }
        UpdateUI();
    }

    public void EnableStartButton(bool enable)
    {
        if (startButton != null)
        {
            bool inGrassArea = shopManager != null && !shopManager.IsInCaveArea();
            startButton.SetActive(enable && inGrassArea);
            Button buttonComponent = startButton.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.interactable = enable && !isWaveActive && inGrassArea;
            }
            UpdateStartButtonSprite();
        }
    }

    public void StartWave()
    {
        if (!isWaveActive && !isGameOver && startButton.activeSelf)
        {
            if (shopManager != null && shopManager.IsInCaveArea()) return;
            isWaveActive = true;
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
        EnableStartButton(true);
        UpdateUI();
        if (shopManager != null)
        {
            shopManager.UpdateMineButton();
        }
    }

    public void AddTower(Vector3 position, int index)
    {
        placedTowers.Add((position, index));
    }

    public List<(Vector3, int)> GetTowers()
    {
        return new List<(Vector3, int)>(placedTowers);
    }

    private void UpdateStartButtonSprite()
    {
        if (startButton != null && startButton.activeSelf)
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
        if (waveText != null) waveText.text = "Wave: " + currentWave + "/" + totalWaves;
        if (healthText != null) healthText.text = "Health: " + (isGameOver ? 0 : playerHealth);
        if (hintText != null && !isWaveActive) // Only update hint when wave is inactive
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

    public void IncrementWave()
    {
        // No longer needed to increment here, handled in StartWave
    }
}