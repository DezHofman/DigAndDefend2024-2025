using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TowerPlacement : MonoBehaviour
{
    public GameObject[] towerPrefabs;
    public int[] copperCosts = { 25, 25, 20, 20, 15 };
    public int[] ironCosts = { 10, 15, 10, 10, 5 };
    [SerializeField] private Tilemap pathTilemap;
    [SerializeField] private Tilemap bigRocksTilemap;
    [SerializeField] private GameObject placementPreviewPrefab;
    [SerializeField] private float canPlaceOpacity;
    [SerializeField] private float cannotPlaceOpacity;
    [SerializeField] private TileBase[] pathTiles;
    [SerializeField] private Sprite barricadeSprite;
    [SerializeField] private Sprite shopSprite;
    [SerializeField] private ShopManager shopManager;

    private int selectedTowerIndex = -1;
    private GameObject placementPreview;
    private SpriteRenderer spriteRenderer;
    private GameObject rangeIndicatorPreview; // New for range during placement
    private const float MINIMUM_TOWER_DISTANCE = 1.5f;
    private int lastCopperCost = 0;
    private int lastIronCost = 0;

    private void Awake()
    {
        // Removed DontDestroyOnLoad
    }

    private void Start()
    {
        if (placementPreview == null)
        {
            placementPreview = Instantiate(placementPreviewPrefab);
            spriteRenderer = placementPreview.GetComponent<SpriteRenderer>();
            placementPreview.SetActive(false);
        }
        if (pathTilemap == null) pathTilemap = GameObject.Find("PATHWAY")?.GetComponent<Tilemap>();
        if (bigRocksTilemap == null) bigRocksTilemap = GameObject.Find("ROCK_BLOCKS")?.GetComponent<Tilemap>();
        if (pathTilemap == null || bigRocksTilemap == null)
        {
            Debug.LogError("TowerPlacement: Path or Big Rocks tilemap not found!");
        }
        else
        {
            Debug.Log("TowerPlacement: Tilemaps successfully initialized.");
        }
        RestoreTowers();
    }

    void Update()
    {
        if (shopManager == null)
        {
            Debug.LogError("TowerPlacement: ShopManager reference is null!");
            if (placementPreview != null)
            {
                placementPreview.SetActive(false);
            }
            if (rangeIndicatorPreview != null)
            {
                rangeIndicatorPreview.SetActive(false);
            }
            return;
        }

        Debug.Log($"IsInCaveArea: {shopManager.IsInCaveArea()}");
        if (shopManager.IsInCaveArea())
        {
            Debug.Log("TowerPlacement: In cave area, disabling placement.");
            if (placementPreview != null)
            {
                placementPreview.SetActive(false);
            }
            if (rangeIndicatorPreview != null)
            {
                rangeIndicatorPreview.SetActive(false);
            }
            return;
        }

        if (pathTilemap == null || bigRocksTilemap == null)
        {
            Debug.LogWarning("TowerPlacement: Tilemap is null, skipping Update.");
            if (placementPreview != null)
            {
                placementPreview.SetActive(false);
            }
            if (rangeIndicatorPreview != null)
            {
                rangeIndicatorPreview.SetActive(false);
            }
            return;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = pathTilemap.WorldToCell(mousePos);
        Vector3 placementPosition = pathTilemap.CellToWorld(cellPosition);
        placementPosition += new Vector3(0.5f, 0.5f, 0f);

        if (selectedTowerIndex >= 0)
        {
            if (placementPreview == null)
            {
                placementPreview = Instantiate(placementPreviewPrefab);
                spriteRenderer = placementPreview.GetComponent<SpriteRenderer>();
            }
            placementPreview.SetActive(true);
            placementPreview.transform.position = placementPosition;

            Sprite towerSprite = GetTowerSprite(selectedTowerIndex, cellPosition);
            if (towerSprite != null)
            {
                spriteRenderer.sprite = towerSprite;
            }
            else
            {
                Debug.LogWarning($"Tower sprite for index {selectedTowerIndex} is null at position {cellPosition}");
            }

            // Show range indicator during placement
            if (rangeIndicatorPreview == null)
            {
                Tower towerPrefab = towerPrefabs[selectedTowerIndex].GetComponent<Tower>();
                if (towerPrefab != null && towerPrefab.rangeIndicatorPrefab != null)
                {
                    rangeIndicatorPreview = Instantiate(towerPrefab.rangeIndicatorPrefab, placementPosition, Quaternion.identity);
                    rangeIndicatorPreview.transform.localScale = new Vector3(towerPrefab.attackRange * 2, towerPrefab.attackRange * 2, 1);
                    rangeIndicatorPreview.layer = LayerMask.NameToLayer("Ignore Raycast");
                }
            }
            if (rangeIndicatorPreview != null)
            {
                rangeIndicatorPreview.transform.position = placementPosition;
                rangeIndicatorPreview.SetActive(true);
            }

            bool isOnPath = pathTilemap.HasTile(cellPosition);
            bool isOnBigRocks = bigRocksTilemap.HasTile(cellPosition);
            bool canPlace = false;
            bool hasMinimumDistance = HasMinimumDistance(placementPosition);

            if (selectedTowerIndex == 4) // Barricade
            {
                if (isOnPath && !isOnBigRocks)
                {
                    canPlace = true;
                    Debug.Log("Barricade: Valid placement spot on path.");
                }
                else
                {
                    Debug.Log($"Barricade: Invalid placement. IsOnPath: {isOnPath}, IsOnBigRocks: {isOnBigRocks}");
                }
            }
            else // Towers
            {
                if (!isOnPath && !isOnBigRocks && hasMinimumDistance)
                {
                    canPlace = true;
                    Debug.Log("Tower: Valid placement spot off path.");
                }
            }

            spriteRenderer.color = canPlace ? new Color(0f, 1f, 0f, canPlaceOpacity) : new Color(1f, 0f, 0f, cannotPlaceOpacity);
            Debug.Log($"Can place: {canPlace}, IsOnPath: {isOnPath}, IsOnBigRocks: {isOnBigRocks}, HasMinimumDistance: {hasMinimumDistance}");
        }
        else
        {
            if (placementPreview != null)
            {
                placementPreview.SetActive(false);
            }
            if (rangeIndicatorPreview != null)
            {
                rangeIndicatorPreview.SetActive(false);
            }
        }

        if (Input.GetMouseButtonDown(0) && selectedTowerIndex >= 0)
        {
            bool isOnPath = pathTilemap.HasTile(cellPosition);
            bool isOnBigRocks = bigRocksTilemap.HasTile(cellPosition);
            bool canPlace = false;
            bool hasMinimumDistance = HasMinimumDistance(placementPosition);

            if (selectedTowerIndex == 4) // Barricade
            {
                if (isOnPath && !isOnBigRocks)
                {
                    canPlace = true;
                    Debug.Log("Barricade: Placement confirmed.");
                }
            }
            else // Towers
            {
                if (!isOnPath && !isOnBigRocks && hasMinimumDistance)
                {
                    canPlace = true;
                    Debug.Log("Tower: Placement confirmed.");
                }
            }

            if (canPlace)
            {
                // Only deduct resources if ShopManager hasn't already
                if (lastCopperCost > 0 || lastIronCost > 0) // Check if resources were tracked (likely by ShopManager)
                {
                    Debug.Log($"Resources already deducted by ShopManager: {lastCopperCost} Copper, {lastIronCost} Iron");
                }
                else
                {
                    if (ResourceManager.Instance.SpendResources(copperCosts[selectedTowerIndex], ironCosts[selectedTowerIndex]))
                    {
                        Debug.Log($"Deducted resources: {copperCosts[selectedTowerIndex]} Copper, {ironCosts[selectedTowerIndex]} Iron");
                    }
                    else
                    {
                        Debug.Log("Not enough resources to place.");
                        return;
                    }
                }

                Vector3 position = placementPosition;
                GameObject tower = Instantiate(towerPrefabs[selectedTowerIndex], position, Quaternion.identity);
                if (selectedTowerIndex == 4)
                {
                    ApplyBarricadeRotation(tower, cellPosition);
                    Debug.Log($"Instantiated barricade at {position}");
                }
                else
                {
                    Debug.Log($"Instantiated tower at {position}");
                }
                GameManager.Instance.AddTower(position, selectedTowerIndex);
                selectedTowerIndex = -1;
                if (rangeIndicatorPreview != null)
                {
                    Destroy(rangeIndicatorPreview);
                    rangeIndicatorPreview = null;
                }
                lastCopperCost = 0;
                lastIronCost = 0;
            }
            else
            {
                string reason = "Cannot place here! ";
                if (isOnPath) reason += "Towers cannot be on Path. ";
                if (isOnBigRocks) reason += "Cannot place on Big Rocks. ";
                if (!hasMinimumDistance && selectedTowerIndex != 4) reason += "Must have a 1.5-unit gap between towers. ";
                if (selectedTowerIndex == 4 && !isOnPath) reason += "Barricades can only be on Path.";
                Debug.Log(reason);
            }
        }
        else if (Input.GetMouseButtonDown(1) && selectedTowerIndex >= 0) // Right-click to cancel
        {
            selectedTowerIndex = -1;
            if (placementPreview != null)
            {
                placementPreview.SetActive(false);
            }
            if (rangeIndicatorPreview != null)
            {
                Destroy(rangeIndicatorPreview);
                rangeIndicatorPreview = null;
            }
            ResourceManager.Instance.AddCopper(lastCopperCost);
            ResourceManager.Instance.AddIron(lastIronCost);
            ShopManager shop = FindFirstObjectByType<ShopManager>();
            if (shop != null)
            {
                shop.CloseShop();
            }
            lastCopperCost = 0;
            lastIronCost = 0;
            Debug.Log("Placement cancelled");
        }
    }

    private bool HasMinimumDistance(Vector3 position)
    {
        GameObject[] towers = GameObject.FindGameObjectsWithTag("Tower");
        foreach (GameObject tower in towers)
        {
            float distance = Vector2.Distance(position, tower.transform.position);
            if (distance < MINIMUM_TOWER_DISTANCE)
            {
                Debug.Log($"TowerPlacement: Position {position} is too close ({distance} units) to tower at {tower.transform.position}.");
                return false;
            }
        }
        return true;
    }

    public void SetSelectedTowerIndex(int index)
    {
        selectedTowerIndex = index;
        if (index >= 0 && index < copperCosts.Length)
        {
            lastCopperCost = copperCosts[index];
            lastIronCost = ironCosts[index];
        }
        Debug.Log($"Selected tower index: {selectedTowerIndex}, Costs: {lastCopperCost} Copper, {lastIronCost} Iron");
    }

    private Sprite GetTowerSprite(int towerIndex, Vector3Int cellPosition)
    {
        if (towerIndex < 0 || towerIndex >= towerPrefabs.Length)
        {
            Debug.LogWarning($"Invalid tower index: {towerIndex}");
            return null;
        }

        if (towerIndex != 4)
        {
            GameObject towerPrefab = towerPrefabs[towerIndex];
            SpriteRenderer spriteRenderer = towerPrefab.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                return spriteRenderer.sprite;
            }
            Debug.LogWarning($"No SpriteRenderer found for tower prefab at index {towerIndex}");
        }
        else
        {
            return GetBarricadeSprite(cellPosition);
        }
        return null;
    }

    private Sprite GetBarricadeSprite(Vector3Int cellPosition)
    {
        if (pathTiles == null || pathTiles.Length == 0 || barricadeSprite == null || shopSprite == null)
        {
            Debug.LogWarning("TowerPlacement: Path tiles or barricade sprites not fully assigned!");
            return barricadeSprite;
        }

        TileBase currentTile = pathTilemap.GetTile(cellPosition);
        if (currentTile == null)
        {
            Debug.Log($"No tile found at {cellPosition}, using default barricade sprite");
            return barricadeSprite;
        }

        for (int i = 0; i < pathTiles.Length; i++)
        {
            if (currentTile == pathTiles[i])
            {
                return (i == 1) ? barricadeSprite : shopSprite;
            }
        }
        Debug.LogWarning($"Unrecognized path tile at {cellPosition}, using default barricade sprite");
        return barricadeSprite;
    }

    private void ApplyBarricadeRotation(GameObject barricade, Vector3Int cellPosition)
    {
        SpriteRenderer barricadeRenderer = barricade.GetComponentInChildren<SpriteRenderer>();
        if (barricadeRenderer == null)
        {
            Debug.LogWarning($"No SpriteRenderer found on barricade at {cellPosition}");
            return;
        }

        barricadeRenderer.sprite = GetBarricadeSprite(cellPosition);
        TileBase currentTile = pathTilemap.GetTile(cellPosition);
        if (currentTile == null)
        {
            Debug.LogWarning($"No tile found at {cellPosition} for rotation");
            return;
        }

        int tileIndex = -1;
        if (pathTiles != null)
        {
            for (int i = 0; i < pathTiles.Length; i++)
            {
                if (pathTiles[i] == currentTile)
                {
                    tileIndex = i;
                    break;
                }
            }
        }

        if (tileIndex == -1)
        {
            Debug.LogWarning($"Tile at {cellPosition} not found in pathTiles, using default rotation");
            barricadeRenderer.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            switch (tileIndex)
            {
                case 0:
                    barricadeRenderer.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case 1:
                    barricadeRenderer.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case 2:
                    barricadeRenderer.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case 3:
                    barricadeRenderer.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case 4:
                    barricadeRenderer.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case 5:
                    barricadeRenderer.transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                default:
                    barricadeRenderer.transform.rotation = Quaternion.Euler(0, 0, 0);
                    Debug.LogWarning($"Unexpected tile index {tileIndex} at {cellPosition}");
                    break;
            }
        }
        Debug.Log($"Applied rotation to barricade at {cellPosition} with index {tileIndex}");
    }

    private void RestoreTowers()
    {
        List<(Vector3, int)> towers = GameManager.Instance.GetTowers();
        foreach (var (position, index) in towers)
        {
            GameObject tower = Instantiate(towerPrefabs[index], position, Quaternion.identity);
            if (index == 4)
            {
                Vector3Int cellPosition = pathTilemap.WorldToCell(position);
                ApplyBarricadeRotation(tower, cellPosition);
            }
        }
    }
}