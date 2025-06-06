using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
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
    [SerializeField] private GameObject miningPlacementPreviewPrefab;
    [SerializeField] private float canPlaceOpacity;
    [SerializeField] private float cannotPlaceOpacity;

    private int currentIndex = 0;
    private string[] grassItemNames = { "Archer Tower", "Bomb Tower", "Slow Tower", "Fire/Poison Tower", "Barricade" };
    private string mineTowerName = "Mining Machine";
    private int mineTowerCopperCost = 0;
    private int mineTowerIronCost = 0;
    private bool isInCaveScene = false;
    private GameObject miningPlacementPreview;
    private SpriteRenderer miningPreviewRenderer;
    private bool isPlacingMiningTower = false;
    private int lastCopperCost = 0;
    private int lastIronCost = 0;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (towerSprites.Length != towerPlacement.towerPrefabs.Length)
        {
            Debug.LogWarning("ShopManager: towerSprites array length does not match towerPrefabs length!");
        }

        if (miningPlacementPreview == null)
        {
            miningPlacementPreview = Instantiate(miningPlacementPreviewPrefab);
            miningPreviewRenderer = miningPlacementPreview.GetComponent<SpriteRenderer>();
            miningPlacementPreview.SetActive(false);
            DontDestroyOnLoad(miningPlacementPreview);
        }

        // Ensure time scale is normal
        if (Time.timeScale != 1f)
        {
            Debug.LogWarning("ShopManager: Time.timeScale was not 1, resetting to 1.");
            Time.timeScale = 1f;
        }

        // Ensure EventSystem exists
        if (FindObjectOfType<EventSystem>() == null)
        {
            Debug.LogWarning("ShopManager: No EventSystem found in scene! Adding one.");
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            DontDestroyOnLoad(eventSystem);
        }

        openedShop.SetActive(false);
        closedShop.SetActive(true);
        SetupButtonListeners();
        UpdateMineButton();
        UpdateShopContent();
    }

    private void SetupButtonListeners()
    {
        if (openButton != null)
        {
            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(() =>
            {
                Debug.Log("ShopManager: Open button clicked!");
                OpenShop();
            });
            openButton.gameObject.SetActive(true);
            EnsureButtonInteractable(openButton);
        }
        else
        {
            Debug.LogError("ShopManager: openButton is not assigned!");
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() =>
            {
                Debug.Log("ShopManager: Close button clicked!");
                CloseShop();
            });
            EnsureButtonInteractable(closeButton);
        }
        else
        {
            Debug.LogError("ShopManager: closeButton is not assigned!");
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() =>
            {
                Debug.Log("ShopManager: Buy button clicked!");
                StartPlacingTower();
            });
            EnsureButtonInteractable(buyButton);
        }
        else
        {
            Debug.LogError("ShopManager: buyButton is not assigned!");
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() =>
            {
                Debug.Log("ShopManager: Next button clicked!");
                NextItem();
            });
            EnsureButtonInteractable(nextButton);
        }
        else
        {
            Debug.LogError("ShopManager: nextButton is not assigned!");
        }

        if (previousButton != null)
        {
            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(() =>
            {
                Debug.Log("ShopManager: Previous button clicked!");
                PreviousItem();
            });
            EnsureButtonInteractable(previousButton);
        }
        else
        {
            Debug.LogError("ShopManager: previousButton is not assigned!");
        }

        if (mineButton != null)
        {
            mineButton.onClick.RemoveAllListeners();
            EnsureButtonInteractable(mineButton);
        }
        else
        {
            Debug.LogError("ShopManager: mineButton is not assigned!");
        }
    }

    private void EnsureButtonInteractable(Button button)
    {
        if (button != null)
        {
            button.interactable = true;
            var canvasGroup = button.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.ignoreParentGroups = false;
            }
            Debug.Log($"ShopManager: Ensured {button.name} is interactable.");
        }
    }

    private void Update()
    {
        Debug.Log("ShopManager: Update called, scene: " + SceneManager.GetActiveScene().name);
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
                Debug.Log("ShopManager: Placing mining tower!");
                if (ResourceManager.Instance.SpendResources(mineTowerCopperCost, mineTowerIronCost))
                {
                    GameObject miningTower = Instantiate(miningManager.miningMachinePrefab, placementPosition, Quaternion.identity);
                    miningTower.tag = "MiningMachine";
                    miningTower.SetActive(true);
                    CloseShop();
                }
            }
            else if (Input.GetMouseButtonDown(1)) // Right-click to cancel
            {
                Debug.Log("ShopManager: Canceling mining tower placement!");
                isPlacingMiningTower = false;
                miningPlacementPreview.SetActive(false);
                ResourceManager.Instance.AddCopper(lastCopperCost);
                ResourceManager.Instance.AddIron(lastIronCost);
            }
        }
        else
        {
            miningPlacementPreview.SetActive(false);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("ShopManager: Scene loaded - " + scene.name);
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
        if (openButton != null)
        {
            openButton.gameObject.SetActive(true);
            EnsureButtonInteractable(openButton);
        }
        if (mineButton != null)
        {
            EnsureButtonInteractable(mineButton);
        }
        isPlacingMiningTower = false;
    }

    private void UpdateMineButton()
    {
        if (mineButton == null)
        {
            Debug.LogWarning("ShopManager: mineButton is not assigned!");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        mineButton.onClick.RemoveAllListeners();
        bool waveActive = GameManager.Instance != null && GameManager.Instance.isWaveActive;
        mineButton.interactable = !waveActive || currentScene == "CaveScene";

        if (currentScene == "GrassScene")
        {
            mineButton.image.sprite = goToMineSprite;
            mineButton.image.color = goToMineColor;
            if (!waveActive)
            {
                mineButton.onClick.AddListener(() =>
                {
                    Debug.Log("ShopManager: Loading CaveScene...");
                    SceneManager.LoadScene("CaveScene");
                });
            }
            else
            {
                Debug.Log("ShopManager: Cannot enter CaveScene during an active wave!");
            }
        }
        else if (currentScene == "CaveScene")
        {
            mineButton.image.sprite = goBackSprite;
            mineButton.image.color = goBackColor;
            mineButton.onClick.AddListener(() =>
            {
                Debug.Log("ShopManager: Loading GrassScene... (Home Button Clicked)");
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
        Debug.Log("ShopManager: Opening shop!");
        UpdateShopContent();
        openedShop.SetActive(true);
        closedShop.SetActive(false);
        if (openButton != null)
        {
            openButton.gameObject.SetActive(false);
        }
    }

    public void CloseShop()
    {
        Debug.Log("ShopManager: Closing shop!");
        isPlacingMiningTower = false;
        if (miningPlacementPreview != null)
        {
            miningPlacementPreview.SetActive(false);
        }
        openedShop.SetActive(false);
        closedShop.SetActive(true);
        if (openButton != null)
        {
            openButton.gameObject.SetActive(true);
            EnsureButtonInteractable(openButton);
        }
    }

    private void StartPlacingTower()
    {
        if (isInCaveScene)
        {
            lastCopperCost = mineTowerCopperCost;
            lastIronCost = mineTowerIronCost;
            if (ResourceManager.Instance.SpendResources(lastCopperCost, lastIronCost))
            {
                isPlacingMiningTower = true;
            }
        }
        else
        {
            if (currentIndex >= 0 && currentIndex < towerPlacement.towerPrefabs.Length)
            {
                lastCopperCost = towerPlacement.copperCosts[currentIndex];
                lastIronCost = towerPlacement.ironCosts[currentIndex];
                if (ResourceManager.Instance.SpendResources(lastCopperCost, lastIronCost))
                {
                    towerPlacement.SetSelectedTowerIndex(currentIndex);
                    CloseShop();
                }
                else
                {
                    Debug.Log($"Not enough resources! Need {lastCopperCost} Copper and {lastIronCost} Iron.");
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