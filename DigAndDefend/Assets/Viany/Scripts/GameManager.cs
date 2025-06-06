using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

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

    private int playerHealth = 100;
    public bool isWaveActive { get; private set; } = false;
    private List<(Vector3, int)> placedTowers = new List<(Vector3, int)>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GrassScene")
        {
            isGameOver = false;
            currentWave = 0;
            playerHealth = 100;
            isWaveActive = false;
            UpdateUI();
            EnableStartButton(true);
        }
        else if (scene.name == "CaveScene")
        {
            UpdateUI();
            EnableStartButton(false); // Disable start button in CaveScene
        }
    }

    public void TakeDamage(int damage)
    {
        if (isGameOver) return;

        playerHealth -= damage;
        Debug.Log($"Player took {damage} damage. Health: {playerHealth}");
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
            startButton.SetActive(enable);
            Button buttonComponent = startButton.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.interactable = enable && !isWaveActive;
            }
            UpdateStartButtonSprite();
        }
    }

    public void StartWave()
    {
        if (!isWaveActive && !isGameOver && startButton.activeSelf)
        {
            isWaveActive = true;
            WaveManager waveManager = Object.FindFirstObjectByType<WaveManager>(); // Fix warning
            if (waveManager != null)
            {
                waveManager.StartWave();
            }
            UpdateUI();
        }
    }

    public void WaveComplete()
    {
        isWaveActive = false;
        EnableStartButton(true);
        UpdateUI();
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
            Debug.Log($"Wave incremented to {currentWave}");
            UpdateUI();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}