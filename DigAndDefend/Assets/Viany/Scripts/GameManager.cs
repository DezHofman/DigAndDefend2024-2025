using UnityEngine;
using UnityEngine.UI;
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

    [SerializeField] private Button startWaveButton; // Assign the button in Inspector
    [SerializeField] private Sprite normalSprite;   // Assign the default sprite
    [SerializeField] private Sprite startedSprite;  // Assign the "wave started" sprite
    private Image buttonImage;                      // Reference to the button's Image

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
    }

    private void Start()
    {
        UpdateHealthUI();
        if (startWaveButton != null)
        {
            buttonImage = startWaveButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = normalSprite; // Set initial sprite
            }
            else
            {
                Debug.LogError("Image component not found on startWaveButton.");
            }
        }
        else
        {
            Debug.LogError("startWaveButton not assigned in Inspector.");
        }
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
        if (startWaveButton != null && startWaveButton.interactable)
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

            // Disable the button and change sprite
            startWaveButton.interactable = false;
            if (buttonImage != null && startedSprite != null)
            {
                buttonImage.sprite = startedSprite;
            }
            else
            {
                Debug.LogWarning("Failed to change sprite: buttonImage or startedSprite is null.");
            }
        }
    }

    public void EnableStartButton()
    {
        if (startWaveButton != null && !isGameOver && currentWave < totalWaves)
        {
            startWaveButton.interactable = true;
            if (buttonImage != null && normalSprite != null)
            {
                buttonImage.sprite = normalSprite;
            }
            else
            {
                Debug.LogWarning("Failed to revert sprite: buttonImage or normalSprite is null.");
            }
        }
    }
}