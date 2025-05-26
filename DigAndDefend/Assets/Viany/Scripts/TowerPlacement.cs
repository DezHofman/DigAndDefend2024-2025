using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerPlacement : MonoBehaviour
{
    public GameObject[] towerPrefabs; // Array of 4 towers and 1 barricade (0: Bomb, 1: Archer, 2: Slow, 3: Fire/Poison, 4: Barricade)
    public int[] copperCosts = { 20, 20, 20, 20, 15 }; // Copper costs for each tower/barricade
    public int[] ironCosts = { 10, 10, 10, 10, 5 };   // Iron costs for each tower/barricade
    public float minDistanceBetweenTowers = 2f;
    [SerializeField] private Tilemap pathTilemap; // Drag the Path Tilemap here
    [SerializeField] private Tilemap bigRocksTilemap; // Drag the BigRocks Tilemap here
    [SerializeField] private GameObject placementPreviewPrefab; // Drag the PlacementPreview prefab here
    private int selectedTowerIndex = -1; // Start with no tower selected (-1)
    private GameObject placementPreview; // The instantiated preview object
    private SpriteRenderer previewRenderer; // The SpriteRenderer of the preview

    private void Start()
    {
        // Instantiate the preview but keep it inactive until a tower is selected
        placementPreview = Instantiate(placementPreviewPrefab);
        previewRenderer = placementPreview.GetComponent<SpriteRenderer>();
        placementPreview.SetActive(false);
    }

    void Update()
    {
        // Select tower type with number keys (1-5), or deselect with 0
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            selectedTowerIndex = -1;
            Debug.Log("Deselected tower/barricade");
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) { selectedTowerIndex = 0; Debug.Log("Selected Bomb Tower"); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { selectedTowerIndex = 1; Debug.Log("Selected Archer Tower"); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { selectedTowerIndex = 2; Debug.Log("Selected Slow Tower"); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { selectedTowerIndex = 3; Debug.Log("Selected Fire/Poison Tower"); }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { selectedTowerIndex = 4; Debug.Log("Selected Barricade"); }

        // Get mouse position and snap to Tilemap grid
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = pathTilemap.WorldToCell(mousePos);
        Vector3 placementPosition = pathTilemap.CellToWorld(cellPosition);
        placementPosition += new Vector3(0.5f, 0.5f, 0f); // Center on the tile

        // Update preview position and visibility
        if (selectedTowerIndex >= 0) // Show preview only if a tower/barricade is selected
        {
            placementPreview.SetActive(true);
            placementPreview.transform.position = placementPosition;

            // Check placement rules for preview color
            bool isOnPath = pathTilemap.HasTile(cellPosition);
            bool isOnBigRocks = bigRocksTilemap.HasTile(cellPosition);
            bool canPlace = false;

            if (selectedTowerIndex == 4) // Barricade
            {
                if (isOnPath && !isOnBigRocks && CanPlaceTower(placementPosition))
                {
                    canPlace = true;
                }
            }
            else // Towers
            {
                if (!isOnPath && !isOnBigRocks && CanPlaceTower(placementPosition))
                {
                    canPlace = true;
                }
            }

            // Check resources for preview color
            if (canPlace)
            {
                canPlace = ResourceManager.Instance.SpendResources(0, 0); // Check without spending
            }

            // Set preview color: Green if can place, Red if cannot
            previewRenderer.color = canPlace ? Color.green : Color.red;
        }
        else
        {
            placementPreview.SetActive(false); // Hide preview if no tower is selected
        }

        // Handle placement on click
        if (Input.GetMouseButtonDown(0) && selectedTowerIndex >= 0)
        {
            bool isOnPath = pathTilemap.HasTile(cellPosition);
            bool isOnBigRocks = bigRocksTilemap.HasTile(cellPosition);
            bool canPlace = false;

            if (selectedTowerIndex == 4) // Barricade
            {
                if (isOnPath && !isOnBigRocks && CanPlaceTower(placementPosition))
                {
                    canPlace = true;
                }
            }
            else // Towers
            {
                if (!isOnPath && !isOnBigRocks && CanPlaceTower(placementPosition))
                {
                    canPlace = true;
                }
            }

            if (canPlace)
            {
                if (ResourceManager.Instance.SpendResources(copperCosts[selectedTowerIndex], ironCosts[selectedTowerIndex]))
                {
                    Instantiate(towerPrefabs[selectedTowerIndex], placementPosition, Quaternion.identity);
                    Debug.Log("Placed at: " + placementPosition);
                    selectedTowerIndex = -1; // Deselect after successful placement
                }
                else
                {
                    Debug.Log("Not enough resources! Need " + copperCosts[selectedTowerIndex] + " Copper and " + ironCosts[selectedTowerIndex] + " Iron.");
                }
            }
            else
            {
                Debug.Log("Cannot place here! Towers cannot be on Path or Big Rocks. Barricades can only be on Path.");
            }
        }
    }

    bool CanPlaceTower(Vector2 position)
    {
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(position, minDistanceBetweenTowers);
        foreach (Collider2D collider in nearbyObjects)
        {
            if (collider.CompareTag("Tower") || collider.CompareTag("Barricade"))
            {
                return false;
            }
        }
        return true;
    }

    private void OnDestroy()
    {
        if (placementPreview != null)
        {
            Destroy(placementPreview);
        }
    }
}