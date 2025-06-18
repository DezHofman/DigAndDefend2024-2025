using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TowerPlacement : MonoBehaviour
{
    public GameObject[] towerPrefabs;
    public int[] copperCosts;
    public int[] ironCosts;
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
    private GameObject rangeIndicatorPreview;
    private const float MINIMUM_TOWER_DISTANCE = 1.5f;
    private int lastCopperCost;
    private int lastIronCost;

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
        RestoreTowers();
    }

    void Update()
    {
        if (shopManager == null)
        {
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

        if (shopManager.IsInCaveArea())
        {
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

            if (selectedTowerIndex == 4)
            {
                if (isOnPath && !isOnBigRocks)
                {
                    canPlace = true;
                }
            }
            else
            {
                if (!isOnPath && !isOnBigRocks && hasMinimumDistance)
                {
                    canPlace = true;
                }
            }

            spriteRenderer.color = canPlace ? new Color(0f, 1f, 0f, canPlaceOpacity) : new Color(1f, 0f, 0f, cannotPlaceOpacity);
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

            if (selectedTowerIndex == 4)
            {
                if (isOnPath && !isOnBigRocks)
                {
                    canPlace = true;
                }
            }
            else
            {
                if (!isOnPath && !isOnBigRocks && hasMinimumDistance)
                {
                    canPlace = true;
                }
            }

            if (canPlace)
            {
                if (lastCopperCost > 0 || lastIronCost > 0)
                {

                }
                else
                {
                    if (!ResourceManager.Instance.SpendResources(copperCosts[selectedTowerIndex], ironCosts[selectedTowerIndex]))
                    {
                        return;
                    }
                }

                Vector3 position = placementPosition;
                GameObject tower = Instantiate(towerPrefabs[selectedTowerIndex], position, Quaternion.identity);
                if (selectedTowerIndex == 4)
                {
                    ApplyBarricadeRotation(tower, cellPosition);
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
        }
        else if (Input.GetMouseButtonDown(1) && selectedTowerIndex >= 0)
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
        if (pathTiles == null || pathTiles.Length == 0 || barricadeSprite == null || shopSprite == null)
        {
            return barricadeSprite;
        }

        TileBase currentTile = pathTilemap.GetTile(cellPosition);
        if (currentTile == null)
        {
            return barricadeSprite;
        }

        for (int i = 0; i < pathTiles.Length; i++)
        {
            if (currentTile == pathTiles[i])
            {
                return (i == 1) ? barricadeSprite : shopSprite;
            }
        }
        return barricadeSprite;
    }

    private void ApplyBarricadeRotation(GameObject barricade, Vector3Int cellPosition)
    {
        SpriteRenderer barricadeRenderer = barricade.GetComponentInChildren<SpriteRenderer>();
        if (barricadeRenderer == null)
        {
            return;
        }

        barricadeRenderer.sprite = GetBarricadeSprite(cellPosition);
        TileBase currentTile = pathTilemap.GetTile(cellPosition);
        if (currentTile == null)
        {
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
                    break;
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