using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public GameObject mushroomPrefab;
    public GameObject golemPrefab;
    public GameObject batPrefab;
    public GameObject ratPrefab;

    public WaveData[] waves;
    public Transform spawnPoint;
    public Transform[] pathWaypoints;
    public float timeBetweenSpawns = 2f;
    public TMP_Text waveText;

    private int currentWaveIndex = -1;
    private int currentEnemyCount = 0;
    private List<EnemyType> currentSpawnOrder;
    private Dictionary<EnemyType, GameObject> enemyTypeToPrefab;

    private void Awake()
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

    private void Start()
    {
        UpdateWaveUI();
    }

    public void StartWave()
    {
        Debug.Log("Starting wave: " + (currentWaveIndex + 1));
        currentWaveIndex++;
        if (currentWaveIndex >= waves.Length || GameManager.Instance.isGameOver)
        {
            Debug.Log("Wave start aborted: index " + currentWaveIndex + " or game over.");
            return;
        }

        WaveData currentWave = waves[currentWaveIndex];
        currentSpawnOrder = new List<EnemyType>(currentWave.enemySpawnOrder);
        currentEnemyCount = 0;
        InvokeRepeating("SpawnEnemy", 0f, timeBetweenSpawns);
        UpdateWaveUI();
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
                        enemyObj.tag = "Enemy"; // Ensure tag is set
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
            if (!GameManager.Instance.isGameOver && GameManager.Instance.currentWave <= GameManager.Instance.totalWaves)
            {
                Debug.Log("Wave complete, enabling start button.");
                GameManager.Instance.EnableStartButton();
            }
            else
            {
                Debug.Log("Wave complete but game over or no more waves.");
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
        waveText.text = "Wave: " + GameManager.Instance.currentWave;
    }
}