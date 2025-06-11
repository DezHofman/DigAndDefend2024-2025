using UnityEngine;
using UnityEngine.Tilemaps;

public class BombTower : Tower
{
    private Tilemap pathTilemap;

    protected override void Start()
    {
        base.Start();
        GameObject pathObject = GameObject.Find("HOME PATH");
        if (pathObject != null)
        {
            pathTilemap = pathObject.GetComponent<Tilemap>();
            if (pathTilemap == null)
            {
                Debug.LogWarning("BombTower: Tilemap component not found on GameObject 'PATH'!");
            }
        }
        else
        {
            Debug.LogWarning("BombTower: Could not find GameObject named 'PATH' in the scene!");
        }
    }

    protected override void HandleAttack(Collider2D[] enemies)
    {
        if (pathTilemap != null && enemies.Length > 0)
        {
            // Find the nearest enemy
            Collider2D nearestEnemy = null;
            float minDistance = float.MaxValue;
            foreach (Collider2D enemy in enemies)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestEnemy = enemy;
                }
            }

            if (nearestEnemy != null)
            {
                // Find the closest path tile to the nearest enemy
                Vector3Int cellPosition = pathTilemap.WorldToCell(nearestEnemy.transform.position);
                BoundsInt bounds = pathTilemap.cellBounds;
                Vector3 closestTilePosition = nearestEnemy.transform.position; // Default to enemy position if no tile found
                float minTileDistance = float.MaxValue;
                bool foundTile = false;

                for (int x = bounds.xMin; x < bounds.xMax; x++)
                {
                    for (int y = bounds.yMin; y < bounds.yMax; y++)
                    {
                        Vector3Int tilePosition = new Vector3Int(x, y, 0);
                        if (pathTilemap.HasTile(tilePosition))
                        {
                            Vector3 worldPosition = pathTilemap.CellToWorld(tilePosition) + new Vector3(0.5f, 0.5f, 0f);
                            float distance = Vector2.Distance(nearestEnemy.transform.position, worldPosition);
                            if (distance < minTileDistance)
                            {
                                minTileDistance = distance;
                                closestTilePosition = worldPosition;
                                foundTile = true;
                            }
                        }
                    }
                }

                if (foundTile)
                {
                    GameObject bomb = Instantiate(projectilePrefab, transform.position + Vector3.up, Quaternion.identity);
                    BombProjectile bombProj = bomb.GetComponent<BombProjectile>();
                    if (bombProj != null)
                    {
                        bombProj.SetTargetPosition(closestTilePosition);
                        bombProj.SetExplosionRadius(2f);
                        Debug.Log($"BombTower: Bomb instantiated at {bomb.transform.position}, targeting {closestTilePosition} near enemy at {nearestEnemy.transform.position}");
                    }
                }
                else
                {
                    Debug.LogWarning("BombTower: No path tiles found near the nearest enemy!");
                }
            }
        }
        else
        {
            Debug.LogWarning("BombTower: No enemies or path Tilemap not found!");
        }
    }
}