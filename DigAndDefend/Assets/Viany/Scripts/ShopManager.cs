using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private GameObject closedShop;
    [SerializeField] private GameObject openedShop;
    [SerializeField] private TowerPlacement towerPlacement;
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button mineButton;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image towerSpriteImage;
    [SerializeField] private Sprite[] towerSprites;
    [SerializeField] private Sprite mineTowerSprite;
    [SerializeField] private Sprite goToMineSprite;
    [SerializeField] private Sprite goBackSprite;
    [SerializeField] private Color goToMineColor = Color.white;
    [SerializeField] private Color goBackColor = Color.gray;
    [SerializeField] private MiningManager miningManager;
    [SerializeField] private GameObject miningPlacementPreviewPrefab; // New: Preview for mining tower
    [SerializeField] private float canPlaceOpacity; // New: Opacity for valid placement
    [SerializeField] private float cannotPlaceOpacity; // New: Opacity for invalid placement

    private int currentIndex = 0;
    private string[] grassItemNames = { "Archer Tower", "Bomb Tower", "Slow Tower", "Fire/Poison Tower", "Barricade" };
    private string mineTowerName = "Mining Machine";
    private int mineTowerCopperCost = 0;
    private int mineTowerIronCost = 0;
    private bool isInCaveScene = false;
    private GameObject miningPlacementPreview; // New: Preview instance
    private SpriteRenderer miningPreviewRenderer; // New: Preview renderer
    private bool isPlacingMiningTower = false; // New: State for placing mining tower

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (towerSprites.Length != towerPlacement.towerPrefabs.Length)
        {
            Debug.LogWarning("ShopManager: towerSprites array length does not match towerPrefabs length!");
        }

        // Initialize mining placement preview
        if (miningPlacementPreview == null)
        {
            miningPlacementPreview = Instantiate(miningPlacementPreviewPrefab);
            miningPreviewRenderer = miningPlacementPreview.GetComponent<SpriteRenderer>();
            miningPlacementPreview.SetActive(false);
            DontDestroyOnLoad(miningPlacementPreview);
        }

        openedShop.SetActive(false);
        openButton.onClick.AddListener(OpenShop);
        closeButton.onClick.AddListener(CloseShop);
        buyButton.onClick.AddListener(StartPlacingMiningTower); // Updated: Start placing mode
        nextButton.onClick.AddListener(NextItem);
        previousButton.onClick.AddListener(PreviousItem);
        mineButton.onClick.RemoveAllListeners();
        UpdateMineButton();
        UpdateShopContent();
    }

    private void Update()
    {
        if (isPlacingMiningTower && isInCaveScene)
        {
            Tilemap minesTilemap = GameObject.Find("MINES")?.GetComponent<Tilemap>();
            if (minesTilemap == null)
            {
                Debug.LogWarning("MINES Tilemap not found!");
                miningPlacementPreview.SetActive(false);
                return;
            }

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = minesTilemap.WorldToCell(mousePos);
            Vector3 placementPosition = minesTilemap.CellToWorld(cellPosition) + new Vector3(0.5f, 0.5f, 0f);

            miningPlacementPreview.SetActive(true);
            miningPlacementPreview.transform.position = placementPosition;
            miningPreviewRenderer.sprite = mineTowerSprite;

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

            miningPreviewRenderer.color = canPlace ? new Color(0f, 1f, 0f, canPlaceOpacity) : new Color(1f, 0f, 0f, cannotPlaceOpacity);

            if (Input.GetMouseButtonDown(0) && canPlace)
            {
                if (ResourceManager.Instance.SpendResources(mineTowerCopperCost, mineTowerIronCost))
                {
                    GameObject miningTower = Instantiate(miningManager.miningMachinePrefab, placementPosition, Quaternion.identity);
                    miningTower.tag = "MiningMachine";
                    miningTower.SetActive(true);
                    isPlacingMiningTower = false;
                    CloseShop();
                }
            }
            else if (Input.GetMouseButtonDown(1)) // Right-click to cancel
            {
                isPlacingMiningTower = false;
                miningPlacementPreview.SetActive(false);
            }
        }
        else
        {
            miningPlacementPreview.SetActive(false);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateMineButton();
        isInCaveScene = scene.name == "CaveScene";
        if (isInCaveScene)
        {
            miningManager = Object.FindFirstObjectByType<MiningManager>();
            if (miningManager == null)
            {
                Debug.LogWarning("MiningManager not found in CaveScene!");
            }
        }
        UpdateShopContent();
        UpdateUIElements();
        closedShop.SetActive(true);
        openButton.gameObject.SetActive(true);
        isPlacingMiningTower = false; // Reset placement mode
    }

    private void UpdateMineButton()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        mineButton.onClick.RemoveAllListeners();
        bool waveActive = GameManager.Instance != null && GameManager.Instance.isWaveActive;
        mineButton.interactable = !waveActive;
        if (currentScene == "GrassScene")
        {
            mineButton.image.sprite = goToMineSprite;
            mineButton.image.color = goToMineColor;
            mineButton.onClick.AddListener(() =>
            {
                if (!waveActive)
                {
                    Debug.Log("Loading CaveScene...");
                    SceneManager.LoadScene("CaveScene");
                }
            });
        }
        else if (currentScene == "CaveScene")
        {
            mineButton.image.sprite = goBackSprite;
            mineButton.image.color = goBackColor;
            mineButton.onClick.AddListener(() =>
            {
                Debug.Log("Loading GrassScene... (Home Button Clicked)");
                SceneManager.LoadScene("GrassScene");
            });
        }
    }

    private void UpdateShopContent()
    {
        if (isInCaveScene)
        {
            itemNameText.text = mineTowerName;
            costText.text = $"{mineTowerCopperCost} Copper, {mineTowerIronCost} Iron";
            towerSpriteImage.sprite = mineTowerSprite;
            nextButton.interactable = false;
            previousButton.interactable = false;
            currentIndex = -1;
        }
        else
        {
            currentIndex = Mathf.Clamp(currentIndex, 0, towerPlacement.towerPrefabs.Length - 1);
            itemNameText.text = grassItemNames[currentIndex];
            int copperCost = towerPlacement.copperCosts[currentIndex];
            int ironCost = towerPlacement.ironCosts[currentIndex];
            costText.text = $"{copperCost} Copper, {ironCost} Iron";
            if (towerSpriteImage != null && currentIndex < towerSprites.Length)
            {
                towerSpriteImage.sprite = towerSprites[currentIndex];
            }
            nextButton.interactable = true;
            previousButton.interactable = true;
        }
    }

    private void UpdateUIElements()
    {
        TextMeshProUGUI copperText = GameObject.Find("CopperText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI ironText = GameObject.Find("IronText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI waveText = GameObject.Find("WaveText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI healthText = GameObject.Find("HealthText")?.GetComponent<TextMeshProUGUI>();

        if (ResourceManager.Instance != null)
        {
            if (copperText != null) copperText.text = "Copper: " + ResourceManager.Instance.copper;
            if (ironText != null) ironText.text = "Iron: " + ResourceManager.Instance.iron;
        }
        if (GameManager.Instance != null)
        {
            if (waveText != null) waveText.text = "Wave: " + GameManager.Instance.currentWave;
            if (healthText != null) healthText.text = "Health: " + (GameManager.Instance.isGameOver ? 0 : 100);
        }
    }

    private void OpenShop()
    {
        UpdateShopContent();
        openedShop.SetActive(true);
        closedShop.SetActive(false);
        openButton.gameObject.SetActive(false);
    }

    private void CloseShop()
    {
        isPlacingMiningTower = false;
        miningPlacementPreview.SetActive(false);
        openedShop.SetActive(false);
        closedShop.SetActive(true);
        openButton.gameObject.SetActive(true);
    }

    private void StartPlacingMiningTower()
    {
        if (isInCaveScene)
        {
            isPlacingMiningTower = true;
        }
        else
        {
            if (currentIndex >= 0 && currentIndex < towerPlacement.towerPrefabs.Length)
            {
                int copperCost = towerPlacement.copperCosts[currentIndex];
                int ironCost = towerPlacement.ironCosts[currentIndex];
                if (ResourceManager.Instance.SpendResources(copperCost, ironCost))
                {
                    towerPlacement.SetSelectedTowerIndex(currentIndex);
                    CloseShop();
                }
                else
                {
                    Debug.Log($"Not enough resources! Need {copperCost} Copper and {ironCost} Iron.");
                }
            }
        }
    }

    private void NextItem()
    {
        if (!isInCaveScene)
        {
            currentIndex = (currentIndex + 1) % towerPlacement.towerPrefabs.Length;
            UpdateShopContent();
        }
    }

    private void PreviousItem()
    {
        if (!isInCaveScene)
        {
            currentIndex = (currentIndex - 1 + towerPlacement.towerPrefabs.Length) % towerPlacement.towerPrefabs.Length;
            UpdateShopContent();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}