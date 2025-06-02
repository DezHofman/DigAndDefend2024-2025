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
    private static Dictionary<string, Vector2> lastDirections = new Dictionary<string, Vector2>
    {
        { "Golem", Vector2.zero },
        { "Mushroom", Vector2.zero },
        { "Rat", Vector2.zero },
        { "Bat", Vector2.zero }
    };
    private static Dictionary<string, Dictionary<GameObject, int>> lastOrders = new Dictionary<string, Dictionary<GameObject, int>>
    {
        { "Golem", new Dictionary<GameObject, int>() },
        { "Mushroom", new Dictionary<GameObject, int>() },
        { "Rat", new Dictionary<GameObject, int>() },
        { "Bat", new Dictionary<GameObject, int>() }
    };
    private static Dictionary<string, List<float>> lastYPositions = new Dictionary<string, List<float>>
    {
        { "Golem", new List<float>() },
        { "Mushroom", new List<float>() },
        { "Rat", new List<float>() },
        { "Bat", new List<float>() }
    };
    private const int BASE_SORTING_ORDER = 1000;
    private const float DIRECTION_CHANGE_THRESHOLD = 0.5f; // Increased for stability
    private const float POSITION_CHANGE_THRESHOLD = 0.2f; // Minimum Y change to trigger update

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public static void AssignSortingOrder(GameObject enemy, string enemyType, Vector2 initialDirection)
    {
        if (!activeEnemies.ContainsKey(enemyType))
        {
            activeEnemies[enemyType] = new List<GameObject>();
            lastDirections[enemyType] = Vector2.zero;
            lastOrders[enemyType] = new Dictionary<GameObject, int>();
            lastYPositions[enemyType] = new List<float>();
        }

        activeEnemies[enemyType].Add(enemy);
        lastOrders[enemyType][enemy] = BASE_SORTING_ORDER;
        lastYPositions[enemyType].Add(enemy.transform.position.y);
        ReassignOrders(enemyType, initialDirection);
    }

    public static void ReassignOrders(string enemyType, Vector2 referenceDirection)
    {
        if (!activeEnemies.ContainsKey(enemyType)) return;

        // Check for significant direction change
        Vector2 lastDirection = lastDirections[enemyType];
        if (Vector2.Distance(referenceDirection, lastDirection) < DIRECTION_CHANGE_THRESHOLD && lastDirection != Vector2.zero)
        {
            // Check for significant position change
            var enemies = activeEnemies[enemyType];
            List<float> currentYPositions = new List<float>();
            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    currentYPositions.Add(enemy.transform.position.y);
                }
            }

            bool significantChange = false;
            for (int i = 0; i < Mathf.Min(currentYPositions.Count, lastYPositions[enemyType].Count); i++)
            {
                if (Mathf.Abs(currentYPositions[i] - lastYPositions[enemyType][i]) > POSITION_CHANGE_THRESHOLD)
                {
                    significantChange = true;
                    break;
                }
            }

            if (!significantChange) return;
        }

        lastDirections[enemyType] = referenceDirection;

        // Clean up invalid references
        List<GameObject> validEnemies = new List<GameObject>();
        List<float> currentYPositionsCleaned = new List<float>();
        foreach (var enemy in activeEnemies[enemyType])
        {
            if (enemy != null)
            {
                validEnemies.Add(enemy);
                currentYPositionsCleaned.Add(enemy.transform.position.y);
            }
        }

        activeEnemies[enemyType] = validEnemies;
        lastYPositions[enemyType] = currentYPositionsCleaned;
        int count = validEnemies.Count;
        if (count == 0) return;

        // Find Y bounds for normalization
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        foreach (var enemy in validEnemies)
        {
            float y = enemy.transform.position.y;
            minY = Mathf.Min(minY, y);
            maxY = Mathf.Max(maxY, y);
        }

        float yRange = maxY - minY;
        if (yRange < 0.01f) yRange = 0.01f;

        // Assign orders based on Y position
        foreach (var enemy in validEnemies)
        {
            SpriteRenderer sr = enemy.GetComponentInChildren<SpriteRenderer>();
            if (sr == null) continue;

            float y = enemy.transform.position.y;
            float normalizedY = (y - minY) / yRange;

            int order;
            if (referenceDirection.y > 0) // Moving up: lowest Y should be behind (lower order)
            {
                order = BASE_SORTING_ORDER - Mathf.RoundToInt(normalizedY * (count - 1));
            }
            else // Moving left, right, or down: highest Y should be above (higher order)
            {
                order = BASE_SORTING_ORDER - Mathf.RoundToInt((1 - normalizedY) * (count - 1));
            }

            // Only update if the order has changed
            if (lastOrders[enemyType].ContainsKey(enemy) && lastOrders[enemyType][enemy] == order)
            {
                continue;
            }

            lastOrders[enemyType][enemy] = order;
            sr.sortingOrder = order;
            Debug.Log($"Assigned {enemyType} enemy {enemy.name} order: {order}, y: {y}, direction: {referenceDirection}, frame: {Time.frameCount}");
        }
    }

    public static void ReleaseSortingOrder(GameObject enemy, string enemyType)
    {
        if (activeEnemies.ContainsKey(enemyType) && activeEnemies[enemyType].Contains(enemy))
        {
            activeEnemies[enemyType].Remove(enemy);
            lastOrders[enemyType].Remove(enemy);
            Debug.Log($"Released {enemyType} enemy: {enemy.name}, frame: {Time.frameCount}");
            ReassignOrders(enemyType, lastDirections[enemyType]);
        }
    }
}