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
        if (pathTilemap != null)
        {
            // Find the closest path tile to the tower
            Vector3Int cellPosition = pathTilemap.WorldToCell(transform.position);
            BoundsInt bounds = pathTilemap.cellBounds;
            Vector3 closestTilePosition = transform.position;
            float minDistance = float.MaxValue;
            bool foundTile = false;

            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int tilePosition = new Vector3Int(x, y, 0);
                    if (pathTilemap.HasTile(tilePosition))
                    {
                        Vector3 worldPosition = pathTilemap.CellToWorld(tilePosition) + new Vector3(0.5f, 0.5f, 0f);
                        float distance = Vector2.Distance(transform.position, worldPosition);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
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
                    Debug.Log($"BombTower: Bomb instantiated at {bomb.transform.position}, targeting {closestTilePosition}");
                }
            }
            else
            {
                Debug.LogWarning("BombTower: No path tiles found in Tilemap bounds!");
            }
        }
        else
        {
            Debug.LogWarning("BombTower: Path Tilemap not found!");
        }
    }
}