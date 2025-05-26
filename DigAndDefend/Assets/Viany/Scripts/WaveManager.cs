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
    }

    private void Start()
    {
        UpdateWaveUI();
    }

    public void StartWave()
    {
        currentWaveIndex++;
        if (currentWaveIndex >= waves.Length || GameManager.Instance.isGameOver)
        {
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
        if (currentEnemyCount < currentSpawnOrder.Count)
        {
            EnemyType type = currentSpawnOrder[currentEnemyCount];

            if (enemyTypeToPrefab.TryGetValue(type, out GameObject prefab) && prefab != null)
            {
                GameObject enemyObj = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
                BaseEnemy enemy = enemyObj.GetComponent<BaseEnemy>();
                enemy.SetWaypoints(pathWaypoints);
            }

            currentEnemyCount++;
        }
        else
        {
            CancelInvoke("SpawnEnemy");
            Invoke("CheckWaveComplete", timeBetweenSpawns + 1f);
        }
    }

    void CheckWaveComplete()
    {
        if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
        {
            GameManager.Instance.StartNextWave();
        }
    }

    void UpdateWaveUI()
    {
        waveText.text = "Wave: " + GameManager.Instance.currentWave;
    }
}
