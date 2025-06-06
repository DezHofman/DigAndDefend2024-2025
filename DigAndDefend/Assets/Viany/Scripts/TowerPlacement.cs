using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TowerPlacement : MonoBehaviour
{
    public GameObject[] towerPrefabs;
    public int[] copperCosts = { 25, 25, 20, 20, 15 };
    public int[] ironCosts = { 10, 15, 10, 10, 5 };
    [SerializeField] private Tilemap pathTilemap; // Update to your new path tilemap name
    [SerializeField] private Tilemap bigRocksTilemap; // Update to your new big rocks tilemap name
    [SerializeField] private GameObject placementPreviewPrefab;
    [SerializeField] private float canPlaceOpacity;
    [SerializeField] private float cannotPlaceOpacity;
    [SerializeField] private TileBase[] minesTiles; // Update if this is still relevant
    [SerializeField] private Sprite barricadeSprite;
    [SerializeField] private Sprite shopSprite;
    [SerializeField] private ShopManager shopManager;

    private int selectedTowerIndex = -1;
    private GameObject placementPreview;
    private SpriteRenderer spriteRenderer;
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
            // Removed DontDestroyOnLoad(placementPreview)
        }
        // Fallback to GameObject.Find if SerializeField is not set
        if (pathTilemap == null) pathTilemap = GameObject.Find("HOME PATH")?.GetComponent<Tilemap>(); // Replace "HOME PATH" with your new name
        if (bigRocksTilemap == null) bigRocksTilemap = GameObject.Find("HOME BIG ROCKS")?.GetComponent<Tilemap>(); // Replace "HOME BIG ROCKS" with your new name
        if (pathTilemap == null || bigRocksTilemap == null)
        {
            Debug.LogError("TowerPlacement: Path or Big Rocks tilemap not found!");
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
            return;
        }

        Debug.Log($"IsInCaveArea: {shopManager.IsInCaveArea()}"); // Debug to check cave state
        if (shopManager.IsInCaveArea())
        {
            if (placementPreview != null)
            {
                placementPreview.SetActive(false);
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
            return;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = pathTilemap.WorldToCell(mousePos);
        Vector3 placementPosition = pathTilemap.CellToWorld(cellPosition);
        placementPosition += new Vector3(0.5f, 0.5f, 0f);

        if (selectedTowerIndex >= 0)
        {
            if (placementPreview != null)
            {
                placementPreview.SetActive(true);
                placementPreview.transform.position = placementPosition;

                Sprite towerSprite = GetTowerSprite(selectedTowerIndex, cellPosition);
                if (towerSprite != null)
                {
                    spriteRenderer.sprite = towerSprite;
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
                    }
                }
                else // Towers
                {
                    if (!isOnPath && !isOnBigRocks && hasMinimumDistance)
                    {
                        canPlace = true;
                    }
                }

                if (canPlace)
                {
                    canPlace = ResourceManager.Instance.SpendResources(0, 0); // Check if resources are sufficient
                }

                spriteRenderer.color = canPlace ? new Color(0f, 1f, 0f, canPlaceOpacity) : new Color(1f, 0f, 0f, cannotPlaceOpacity);
                Debug.Log($"Can place: {canPlace}, IsOnPath: {isOnPath}, IsOnBigRocks: {isOnBigRocks}, HasMinimumDistance: {hasMinimumDistance}");
            }
        }
        else
        {
            if (placementPreview != null)
            {
                placementPreview.SetActive(false);
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
                }
            }
            else // Towers
            {
                if (!isOnPath && !isOnBigRocks && hasMinimumDistance)
                {
                    canPlace = true;
                }
            }

            if (canPlace)
            {
                if (ResourceManager.Instance.SpendResources(copperCosts[selectedTowerIndex], ironCosts[selectedTowerIndex]))
                {
                    Vector3 position = placementPosition;
                    GameObject tower = Instantiate(towerPrefabs[selectedTowerIndex], position, Quaternion.identity);
                    if (selectedTowerIndex == 4)
                    {
                        ApplyBarricadeRotation(tower, cellPosition);
                    }
                    GameManager.Instance.AddTower(position, selectedTowerIndex);
                    selectedTowerIndex = -1;
                    lastCopperCost = 0;
                    lastIronCost = 0;
                    Debug.Log($"Tower placed at {position} with index {selectedTowerIndex}");
                }
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
        }
        else
        {
            return GetBarricadeSprite(cellPosition);
        }
        return null;
    }

    private Sprite GetBarricadeSprite(Vector3Int cellPosition)
    {
        if (minesTiles == null || minesTiles.Length != 6 || barricadeSprite == null || shopSprite == null)
        {
            Debug.LogWarning("TowerPlacement: Path tiles or barricade sprites not fully assigned!");
            return null;
        }

        TileBase currentTile = pathTilemap.GetTile(cellPosition);
        if (currentTile == null)
        {
            return null;
        }

        if (currentTile == minesTiles[0])
        {
            return shopSprite;
        }
        else if (currentTile == minesTiles[1])
        {
            return barricadeSprite;
        }
        else if (currentTile == minesTiles[2] || currentTile == minesTiles[3] || currentTile == minesTiles[4] || currentTile == minesTiles[5])
        {
            return shopSprite;
        }
        Debug.LogWarning($"TowerPlacement: Unrecognized path tile at {cellPosition}");
        return shopSprite;
    }

    private void ApplyBarricadeRotation(GameObject barricade, Vector3Int cellPosition)
    {
        SpriteRenderer barricadeRenderer = barricade.GetComponentInChildren<SpriteRenderer>();
        if (barricadeRenderer != null)
        {
            barricadeRenderer.sprite = GetBarricadeSprite(cellPosition);
            TileBase currentTile = pathTilemap.GetTile(cellPosition);
            if (currentTile == minesTiles[0])
            {
                barricadeRenderer.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (currentTile == minesTiles[1])
            {
                barricadeRenderer.transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else if (currentTile == minesTiles[2])
            {
                barricadeRenderer.transform.rotation = Quaternion.Euler(0, 0, 45);
            }
            else if (currentTile == minesTiles[3])
            {
                barricadeRenderer.transform.rotation = Quaternion.Euler(0, 0, 135);
            }
            else if (currentTile == minesTiles[4])
            {
                barricadeRenderer.transform.rotation = Quaternion.Euler(0, 0, 135);
            }
            else if (currentTile == minesTiles[5])
            {
                barricadeRenderer.transform.rotation = Quaternion.Euler(0, 0, 45);
            }
        }
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