using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerPlacement : MonoBehaviour
{
    public GameObject[] towerPrefabs; // Order: Archer, Bomb, Slow, Fire/Poison, Barricade
    public int[] copperCosts = { 20, 20, 20, 20, 15 };
    public int[] ironCosts = { 10, 10, 10, 10, 5 };
    [SerializeField] private Tilemap pathTilemap;
    [SerializeField] private Tilemap bigRocksTilemap;
    [SerializeField] private GameObject placementPreviewPrefab;
    [SerializeField] private float canPlaceOpacity;
    [SerializeField] private float cannotPlaceOpacity;
    private int selectedTowerIndex = -1;
    private GameObject placementPreview;
    private SpriteRenderer previewRenderer;

    private void Start()
    {
        placementPreview = Instantiate(placementPreviewPrefab);
        previewRenderer = placementPreview.GetComponent<SpriteRenderer>();
        placementPreview.SetActive(false);
    }

    void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = pathTilemap.WorldToCell(mousePos);
        Vector3 placementPosition = pathTilemap.CellToWorld(cellPosition);
        placementPosition += new Vector3(0.5f, 0.5f, 0f);

        if (selectedTowerIndex >= 0)
        {
            placementPreview.SetActive(true);
            placementPreview.transform.position = placementPosition;

            // Update the preview sprite to match the selected tower
            Sprite towerSprite = GetTowerSprite(selectedTowerIndex);
            if (towerSprite != null)
            {
                previewRenderer.sprite = towerSprite;
                Debug.Log($"TowerPlacement: Preview sprite updated for tower index {selectedTowerIndex}");
            }
            else
            {
                Debug.LogWarning($"TowerPlacement: Could not retrieve sprite for tower index {selectedTowerIndex}");
            }

            bool isOnPath = pathTilemap.HasTile(cellPosition);
            bool isOnBigRocks = bigRocksTilemap.HasTile(cellPosition);
            bool canPlace = false;

            if (selectedTowerIndex == 4) // Barricade
            {
                if (isOnPath && !isOnBigRocks)
                {
                    canPlace = true;
                }
            }
            else
            {
                if (!isOnPath && !isOnBigRocks)
                {
                    canPlace = true;
                }
            }

            if (canPlace)
            {
                canPlace = ResourceManager.Instance.SpendResources(0, 0);
            }

            previewRenderer.color = canPlace ? new Color(0f, 1f, 0f, canPlaceOpacity) : new Color(1f, 0f, 0f, cannotPlaceOpacity);
        }
        else
        {
            placementPreview.SetActive(false);
        }

        if (Input.GetMouseButtonDown(0) && selectedTowerIndex >= 0)
        {
            bool isOnPath = pathTilemap.HasTile(cellPosition);
            bool isOnBigRocks = bigRocksTilemap.HasTile(cellPosition);
            bool canPlace = false;

            if (selectedTowerIndex == 4)
            {
                if (isOnPath && !isOnBigRocks)
                {
                    canPlace = true;
                }
            }
            else
            {
                if (!isOnPath && !isOnBigRocks)
                {
                    canPlace = true;
                }
            }

            if (canPlace)
            {
                if (ResourceManager.Instance.SpendResources(copperCosts[selectedTowerIndex], ironCosts[selectedTowerIndex]))
                {
                    Debug.Log("Instantiating tower at position: " + placementPosition);
                    GameObject tower = Instantiate(towerPrefabs[selectedTowerIndex], placementPosition, Quaternion.identity, null);
                    Debug.Log("Tower position after instantiation: " + tower.transform.position);
                    Debug.Log("Tower parent: " + (tower.transform.parent != null ? tower.transform.parent.name : "None"));
                    Debug.Log("Placed at: " + placementPosition);
                    selectedTowerIndex = -1;
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

    public void SetSelectedTowerIndex(int index)
    {
        selectedTowerIndex = index;
        Debug.Log($"TowerPlacement: Selected tower index set to {index}");
    }

    private Sprite GetTowerSprite(int towerIndex)
    {
        if (towerIndex < 0 || towerIndex >= towerPrefabs.Length)
        {
            return null;
        }

        GameObject towerPrefab = towerPrefabs[towerIndex];
        SpriteRenderer spriteRenderer = towerPrefab.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            return spriteRenderer.sprite;
        }

        return null;
    }

    private void OnDestroy()
    {
        if (placementPreview != null)
        {
            Destroy(placementPreview);
        }
    }
}