using UnityEngine;
using System.Collections.Generic;

public class EnemySortingManager : MonoBehaviour
{
    private static Dictionary<string, List<GameObject>> activeEnemies = new Dictionary<string, List<GameObject>>
    {
        { "Golem", new List<GameObject>() },
        { "Mushroom", new List<GameObject>() },
        { "Rat", new List<GameObject>() },
        { "Bat", new List<GameObject>() }
    };

    private void Awake()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    public static void AssignSortingOrder(GameObject enemy, string enemyType)
    {
        if (!activeEnemies.ContainsKey(enemyType))
        {
            activeEnemies[enemyType] = new List<GameObject>();
        }

        activeEnemies[enemyType].Add(enemy);
    }

    public static void ReleaseSortingOrder(GameObject enemy, string enemyType)
    {
        if (activeEnemies.ContainsKey(enemyType) && activeEnemies[enemyType].Contains(enemy))
        {
            activeEnemies[enemyType].Remove(enemy);
            Debug.Log($"Released {enemyType} enemy: {enemy.name}, frame: {Time.frameCount}");
        }
    }

    public static List<GameObject> GetActiveEnemies(string enemyType)
    {
        return activeEnemies.ContainsKey(enemyType) ? activeEnemies[enemyType] : null;
    }
}