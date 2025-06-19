using UnityEngine;
using UnityEngine.Tilemaps;

public class MiningPlacement : MonoBehaviour
{
    [SerializeField] private MiningManager miningManager;
    [SerializeField] private GameObject placementPreviewPrefab;
    [SerializeField] private float canPlaceOpacity;
    [SerializeField] private float cannotPlaceOpacity;
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private RuntimeAnimatorController animatorController;

    private bool isPlacing;
    private GameObject placementPreview;
    private SpriteRenderer previewRenderer;
    private int lastCopperCost;
    private int lastIronCost;

    private void Start()
    {
        placementPreview = Instantiate(placementPreviewPrefab);
        previewRenderer = placementPreview.GetComponent<SpriteRenderer>();
        placementPreview.SetActive(false);

        placementPreview.GetComponent<Animator>().runtimeAnimatorController = animatorController;
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
            Collider2D hit = Physics2D.OverlapCircle(placementPosition, 0.1f);
            if (hit != null && hit.CompareTag("MiningMachine"))
            {
                canPlace = false;
            }
        }

        previewRenderer.color = canPlace ? new Color(0f, 1f, 0f, canPlaceOpacity) : new Color(1f, 0f, 0f, cannotPlaceOpacity);

        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            if (ResourceManager.Instance.SpendResources(0, 0))
            {
                GameObject miningTower = Instantiate(miningManager.MiningMachinePrefab, placementPosition, Quaternion.identity);
                miningTower.tag = "MiningMachine";
                miningTower.SetActive(true);
                miningManager.ConfigureMiningTower(miningTower);
                isPlacing = false;
                placementPreview.SetActive(false);
            }
        }
        else if (Input.GetMouseButtonDown(1))
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
