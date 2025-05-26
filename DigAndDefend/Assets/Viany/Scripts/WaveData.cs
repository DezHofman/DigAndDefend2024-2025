using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WaveData", menuName = "WaveSystem/WaveData", order = 1)]
public class WaveData : ScriptableObject
{
    public List<EnemyType> enemySpawnOrder;
}
