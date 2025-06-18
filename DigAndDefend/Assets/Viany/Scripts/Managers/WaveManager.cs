using UnityEngine;
using TMPro;
using System.Collections.Generic;

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
    private int currentEnemyCount;
    private List<EnemyType> currentSpawnOrder;
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
        }
    }

    private void Start()
    {
        UpdateWaveUI();
    }

    public void StartWave()
    {
        currentWaveIndex++;
        if (currentWaveIndex >= GameManager.Instance.totalWaves || GameManager.Instance.isGameOver)
        {
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

        if (wave >= 5)
        {
            foreach (GameObject prefab in new[] { mushroomPrefab, batPrefab, ratPrefab, golemPrefab })
            {
                BaseEnemy enemy = prefab.GetComponent<BaseEnemy>();
                if (enemy != null)
                {
                    if (enemy.GetType() == typeof(MushroomEnemy)) enemy.health = 120;
                    else if (enemy.GetType() == typeof(BatEnemy) || enemy.GetType() == typeof(RatEnemy)) enemy.health = 60;
                }
            }
        }
    }

    void SpawnEnemy()
    {
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
                    }
                }
            }

            currentEnemyCount++;
        }
        else
        {
            CancelInvoke("SpawnEnemy");
            CheckWaveComplete();
        }
    }

    void CheckWaveComplete()
    {
        int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (enemyCount == 0)
        {
            if (!GameManager.Instance.isGameOver && GameManager.Instance.currentWave < GameManager.Instance.totalWaves)
            {
                GameManager.Instance.WaveComplete();
            }
            else if (GameManager.Instance.currentWave >= GameManager.Instance.totalWaves)
            {
                gameManager.SetWin();
            }
        }
        else
        {
            Invoke("CheckWaveComplete", 0.5f);
        }
    }

    void UpdateWaveUI()
    {
        if (waveText != null) waveText.text = "Wave: " + GameManager.Instance.currentWave + "/" + GameManager.Instance.totalWaves;
    }
}