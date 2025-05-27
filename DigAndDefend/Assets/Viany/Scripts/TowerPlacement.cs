using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerPlacement : MonoBehaviour
{
    public GameObject[] towerPrefabs;
    public int[] copperCosts = { 20, 20, 20, 20, 15 };
    public int[] ironCosts = { 10, 10, 10, 10, 5 };
    [SerializeField] private Tilemap pathTilemap;
    [SerializeField] private Tilemap bigRocksTilemap;
    [SerializeField] private GameObject placementPreviewPrefab;
    [SerializeField] private float canPlaceOpacity = 0.5f;
    [SerializeField] private float cannotPlaceOpacity = 0.5f;
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
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            selectedTowerIndex = -1;
            Debug.Log("Deselected tower/barricade");
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) { SetSelectedTowerIndex(0); Debug.Log("Selected Archer Tower"); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { SetSelectedTowerIndex(1); Debug.Log("Selected Bomb Tower"); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { SetSelectedTowerIndex(2); Debug.Log("Selected Slow Tower"); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { SetSelectedTowerIndex(3); Debug.Log("Selected Fire/Poison Tower"); }
        if (Input.GetKeyDown(KeyCode.Alpha5)) { SetSelectedTowerIndex(4); Debug.Log("Selected Barricade"); }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = pathTilemap.WorldToCell(mousePos);
        Vector3 placementPosition = pathTilemap.CellToWorld(cellPosition);
        placementPosition += new Vector3(0.5f, 0.5f, 0f);

        if (selectedTowerIndex >= 0)
        {
            placementPreview.SetActive(true);
            placementPreview.transform.position = placementPosition;

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
                    Instantiate(towerPrefabs[selectedTowerIndex], placementPosition, Quaternion.identity);
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
    }

    private void OnDestroy()
    {
        if (placementPreview != null)
        {
            Destroy(placementPreview);
        }
    }
}