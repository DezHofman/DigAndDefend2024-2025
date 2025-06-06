using UnityEngine;
using UnityEngine.Tilemaps;

public class MiningPlacement : MonoBehaviour
{
    [SerializeField] private MiningManager miningManager;
    [SerializeField] private GameObject placementPreviewPrefab;
    [SerializeField] private float canPlaceOpacity;
    [SerializeField] private float cannotPlaceOpacity;
    [SerializeField] private ShopManager shopManager;

    private bool isPlacing = false;
    private GameObject placementPreview;
    private SpriteRenderer previewRenderer;
    private int lastCopperCost = 0;
    private int lastIronCost = 0;

    private void Start()
    {
        if (placementPreview == null)
        {
            placementPreview = Instantiate(placementPreviewPrefab);
            previewRenderer = placementPreview.GetComponent<SpriteRenderer>();
            placementPreview.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isPlacing || shopManager == null || !shopManager.IsInCaveArea())
        {
            if (placementPreview != null)
            {
                placementPreview.SetActive(false);
            }
            return;
        }

        Tilemap minesTilemap = GameObject.Find("MINE MINES")?.GetComponent<Tilemap>();
        if (minesTilemap == null)
        {
            Debug.LogWarning("MINES Tilemap not found!");
            placementPreview.SetActive(false);
            return;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = minesTilemap.WorldToCell(mousePos);
        Vector3 placementPosition = minesTilemap.CellToWorld(cellPosition) + new Vector3(0.5f, 0.5f, 0f);

        placementPreview.SetActive(true);
        placementPreview.transform.position = placementPosition;
        previewRenderer.sprite = miningManager.MiningMachinePrefab.GetComponentInChildren<SpriteRenderer>().sprite;

        bool canPlace = minesTilemap.HasTile(cellPosition);
        if (canPlace)
        {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(placementPosition, 0.1f);
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.CompareTag("MiningMachine"))
                {
                    canPlace = false;
                    break;
                }
            }
        }

        previewRenderer.color = canPlace ? new Color(0f, 1f, 0f, canPlaceOpacity) : new Color(1f, 0f, 0f, cannotPlaceOpacity);

        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            if (ResourceManager.Instance.SpendResources(0, 0)) // Resources already spent on buy
            {
                GameObject miningTower = Instantiate(miningManager.MiningMachinePrefab, placementPosition, Quaternion.identity);
                miningTower.tag = "MiningMachine";
                miningTower.SetActive(true);
                miningManager.ConfigureMiningTower(miningTower); // Configure lifespan and health
                isPlacing = false;
                placementPreview.SetActive(false);
            }
        }
        else if (Input.GetMouseButtonDown(1)) // Right-click to cancel
        {
            isPlacing = false;
            placementPreview.SetActive(false);
            ResourceManager.Instance.AddCopper(lastCopperCost);
            ResourceManager.Instance.AddIron(lastIronCost);
            lastCopperCost = 0;
            lastIronCost = 0;
        }
    }

    public void StartPlacing(int copperCost, int ironCost)
    {
        isPlacing = true;
        lastCopperCost = copperCost;
        lastIronCost = ironCost;
    }
}