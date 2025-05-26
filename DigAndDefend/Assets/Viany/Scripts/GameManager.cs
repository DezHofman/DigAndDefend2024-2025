using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int villageHealth = 100;
    public int currentWave = 0;
    public int totalWaves = 5;
    public bool isGameOver = false;
    public TMP_Text healthText;
    public WaveManager waveManager;

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
        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        villageHealth -= damage;
        if (villageHealth <= 0)
        {
            villageHealth = 0;
            GameOver();
        }
        UpdateHealthUI();
    }

    public void GameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over!");
    }

    void UpdateHealthUI()
    {
        healthText.text = "Health: " + villageHealth;
    }

    public void StartNextWave()
    {
        currentWave++;
        if (currentWave > totalWaves)
        {
            Debug.Log("All waves completed! You win!");
            isGameOver = true;
        }
        else
        {
            waveManager.StartWave();
        }
    }
}