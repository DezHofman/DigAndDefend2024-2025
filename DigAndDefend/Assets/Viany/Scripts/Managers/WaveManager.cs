using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class WaveManager : MonoBehaviour
{
    public GameObject mushroomPrefab;
    public GameObject golemPrefab;
    public GameObject batPrefab;
    public GameObject ratPrefab;

    public Transform spawnPoint;
    public Transform[] pathWaypoints;
    public float timeBetweenSpawns = 2f;
    public TMP_Text waveText;

    private int currentWaveIndex = -1;
    private int currentEnemyCount = 0;
    private List<EnemyType> currentSpawnOrder; // Reference to existing EnemyType enum
    private Dictionary<EnemyType, GameObject> enemyTypeToPrefab;
    public GameManager gameManager;

    private void Awake()
    {
        if (enemyTypeToPrefab == null)
        {
            enemyTypeToPrefab = new Dictionary<EnemyType, GameObject>
            {
                { EnemyType.Mushroom, mushroomPrefab },
                { EnemyType.Golem, golemPrefab },
                { EnemyType.Bat, batPrefab },
                { EnemyType.Rat, ratPrefab }
            };
            Debug.Log("WaveManager initialized with prefabs: " + string.Join(", ", enemyTypeToPrefab.Keys));
        }
    }

    private void Start()
    {
        UpdateWaveUI();
    }

    public void StartWave()
    {
        Debug.Log("Starting wave: " + (currentWaveIndex + 1));
        currentWaveIndex++;
        if (currentWaveIndex >= GameManager.Instance.totalWaves || GameManager.Instance.isGameOver)
        {
            Debug.Log("Wave start aborted: index " + currentWaveIndex + " or game over.");
            return;
        }

        GameManager.Instance.StartWave();
        GenerateWaveEnemies();
        currentEnemyCount = 0;
        InvokeRepeating("SpawnEnemy", 0f, timeBetweenSpawns);
        UpdateWaveUI();
    }

    void GenerateWaveEnemies()
    {
        int wave = GameManager.Instance.currentWave + 1;
        int totalEnemies = Mathf.Clamp(5 + wave * 2, 5, 20);
        currentSpawnOrder = new List<EnemyType>();

        float mushroomWeight = 0.6f - (wave * 0.05f);
        float batWeight = 0.2f + (wave * 0.05f);
        float ratWeight = 0.2f + (wave * 0.05f);
        float golemWeight = (wave >= 4 ? (wave - 3) * 0.05f : 0f);

        float totalWeight = mushroomWeight + batWeight + ratWeight + golemWeight;
        mushroomWeight /= totalWeight;
        batWeight /= totalWeight;
        ratWeight /= totalWeight;
        golemWeight /= totalWeight;

        for (int i = 0; i < totalEnemies; i++)
        {
            float roll = Random.value;
            if (roll < mushroomWeight)
                currentSpawnOrder.Add(EnemyType.Mushroom);
            else if (roll < mushroomWeight + batWeight)
                currentSpawnOrder.Add(EnemyType.Bat);
            else if (roll < mushroomWeight + batWeight + ratWeight)
                currentSpawnOrder.Add(EnemyType.Rat);
            else
                currentSpawnOrder.Add(EnemyType.Golem);
        }

        // Adjust HP based on wave (example)
        if (wave >= 5)
        {
            // Update prefabs' BaseEnemy HP in Awake or a separate method
            foreach (GameObject prefab in new[] { mushroomPrefab, batPrefab, ratPrefab, golemPrefab })
            {
                BaseEnemy enemy = prefab.GetComponent<BaseEnemy>();
                if (enemy != null)
                {
                    if (enemy.GetType() == typeof(MushroomEnemy)) enemy.health = 120;
                    else if (enemy.GetType() == typeof(BatEnemy) || enemy.GetType() == typeof(RatEnemy)) enemy.health = 60;
                    // Golem remains 700
                }
            }
        }

        Debug.Log($"Wave {wave}: Generated {totalEnemies} enemies - {string.Join(", ", currentSpawnOrder)}");
    }

    void SpawnEnemy()
    {
        Debug.Log("Attempting to spawn enemy, count: " + currentEnemyCount + "/" + currentSpawnOrder.Count);
        if (currentEnemyCount < currentSpawnOrder.Count)
        {
            EnemyType type = currentSpawnOrder[currentEnemyCount];

            if (enemyTypeToPrefab.TryGetValue(type, out GameObject prefab) && prefab != null)
            {
                GameObject enemyObj = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
                if (enemyObj != null)
                {
                    BaseEnemy enemy = enemyObj.GetComponent<BaseEnemy>();
                    if (enemy != null)
                    {
                        enemy.SetWaypoints(pathWaypoints);
                        enemyObj.tag = "Enemy";
                        Debug.Log("Spawned " + type + " at " + spawnPoint.position);
                    }
                    else
                    {
                        Debug.LogError("No BaseEnemy component on " + prefab.name);
                    }
                }
                else
                {
                    Debug.LogError("Failed to instantiate " + type);
                }
            }
            else
            {
                Debug.LogError("Prefab not found or null for " + type);
            }

            currentEnemyCount++;
        }
        else
        {
            CancelInvoke("SpawnEnemy");
            Debug.Log("All enemies scheduled, checking completion in " + (timeBetweenSpawns + 1f) + "s");
            Invoke("CheckWaveComplete", timeBetweenSpawns + 1f);
        }
    }

    void CheckWaveComplete()
    {
        int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        Debug.Log("Checking wave complete, enemies remaining: " + enemyCount);
        if (enemyCount == 0)
        {
            if (!GameManager.Instance.isGameOver && GameManager.Instance.currentWave < GameManager.Instance.totalWaves)
            {
                GameManager.Instance.WaveComplete();
                Debug.Log("Wave complete, enabling start button.");
                GameManager.Instance.EnableStartButton(true);
            }
            else
            {
                Debug.Log("Wave complete but game over or no more waves.");
                gameManager.SetWin();
            }
        }
        else
        {
            Debug.Log("Wave not complete, rescheduling check.");
            Invoke("CheckWaveComplete", timeBetweenSpawns + 1f);
        }
    }

    void UpdateWaveUI()
    {
        if (waveText != null) waveText.text = "Wave: " + GameManager.Instance.currentWave;
    }
}