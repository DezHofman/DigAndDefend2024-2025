using UnityEngine;
using UnityEngine.Tilemaps;

public class BombTower : Tower
{
    private Tilemap pathTilemap;

    protected override void Start()
    {
        base.Start();
        GameObject pathObject = GameObject.Find("PATH");
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
        if (pathTilemap == null)
        {
            Debug.LogWarning("BombTower: Path Tilemap not assigned!");
            return;
        }

        Vector3 landingPoint = transform.position; // Default to tower position if no closer tile
        float minDistance = float.MaxValue;

        // Find the closest path tile
        Vector3Int cellPosition = pathTilemap.WorldToCell(transform.position);
        BoundsInt bounds = pathTilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                if (pathTilemap.HasTile(tilePosition))
                {
                    Vector3 worldPosition = pathTilemap.CellToWorld(tilePosition);
                    // Adjust to tile center (assuming 1x1 tile size)
                    worldPosition += new Vector3(0.5f, 0.5f, 0f);
                    float distance = Vector2.Distance(transform.position, worldPosition);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        landingPoint = worldPosition;
                    }
                }
            }
        }

        SpawnProjectile(null, landingPoint);
    }
}