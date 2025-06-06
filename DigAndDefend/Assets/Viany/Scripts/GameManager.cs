using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool isGameOver { get; private set; } = false;
    public int currentWave { get; private set; } = 0;
    public int totalWaves { get; private set; } = 10;
    [SerializeField] private GameObject startButton;
    [SerializeField] private Sprite startSprite;
    [SerializeField] private Sprite activeWaveSprite;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private ShopManager shopManager;

    private int playerHealth = 100;
    public bool isWaveActive { get; private set; } = false;
    private List<(Vector3, int)> placedTowers = new List<(Vector3, int)>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Removed DontDestroyOnLoad
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        isGameOver = false;
        currentWave = 0;
        playerHealth = 100;
        isWaveActive = false;
        UpdateUI();
        EnableStartButton(true);
        if (startButton != null)
        {
            startButton.SetActive(true);
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
        if (waveText != null) waveText.text = "Wave: " + currentWave;
        if (healthText != null) healthText.text = "Health: " + (isGameOver ? 0 : playerHealth);
    }

    public void IncrementWave()
    {
        if (!isGameOver)
        {
            currentWave++;
            UpdateUI();
        }
    }
}