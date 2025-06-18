using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

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
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Vector2 grassCameraPosition = new Vector2(0f, 0f);
    [SerializeField] private Vector2 caveCameraPosition = new Vector2(100f, 0f);
    [SerializeField] private MiningPlacement miningPlacement; // New reference

    [SerializeField] private Image lockOverlay; // Opacity overlay for locked towers
    [SerializeField] private Image lockImage; // Lock icon for locked towers
    [SerializeField] private TextMeshProUGUI unlockText; // Text for "Unlock at Wave X"
    [SerializeField] private int[] unlockWaves; // Editable array of wave numbers for tower unlocks

    private int currentIndex = 0;
    private string[] grassItemNames = { "Archer Tower", "Bomb Tower", "Slow Tower", "Fire Tower", "Barricade" };
    private string mineTowerName = "Mining Machine";
    private int mineTowerCopperCost = 0;
    private int mineTowerIronCost = 0;
    private bool isInCaveArea = false;

    public bool IsInCaveArea() => isInCaveArea;

    private void Start()
    {
        if (towerSprites.Length != towerPlacement.towerPrefabs.Length)
        {
            Debug.LogWarning("ShopManager: towerSprites array length does not match towerPrefabs length!");
        }

        if (unlockWaves == null || unlockWaves.Length != towerPlacement.towerPrefabs.Length)
        {
            Debug.LogWarning("ShopManager: unlockWaves array length does not match towerPrefabs length! Initializing with default values.");
            unlockWaves = new int[towerPlacement.towerPrefabs.Length];
            for (int i = 0; i < unlockWaves.Length; i++)
            {
                unlockWaves[i] = (i + 1) * 5; // Default: Wave 5, 10, 15, etc.
            }
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("ShopManager: Main Camera not found!");
            }
        }

        openedShop.SetActive(false);
        closedShop.SetActive(true);
        SetupButtonListeners();
        UpdateMineButton();
        UpdateShopContent();
        MoveCameraToGrass();
    }

    private void SetupButtonListeners()
    {
        if (openButton != null)
        {
            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(OpenShop);
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
            closeButton.onClick.AddListener(CloseShop);
            EnsureButtonInteractable(closeButton);
        }
        else
        {
            Debug.LogError("ShopManager: closeButton is not assigned!");
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(StartPlacingTower);
            EnsureButtonInteractable(buyButton);
        }
        else
        {
            Debug.LogError("ShopManager: buyButton is not assigned!");
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextItem);
            EnsureButtonInteractable(nextButton);
        }
        else
        {
            Debug.LogError("ShopManager: nextButton is not assigned!");
        }

        if (previousButton != null)
        {
            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(PreviousItem);
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
        }
    }

    public void UpdateMineButton()
    {
        if (mineButton == null)
        {
            Debug.LogWarning("ShopManager: mineButton is not assigned!");
            return;
        }

        bool waveActive = GameManager.Instance != null && GameManager.Instance.isWaveActive;
        Debug.Log($"UpdateMineButton: isWaveActive = {waveActive}, isInCaveArea = {isInCaveArea}");
        mineButton.onClick.RemoveAllListeners();

        if (!isInCaveArea)
        {
            mineButton.image.sprite = goToMineSprite;
            mineButton.image.color = goToMineColor;
            mineButton.interactable = !waveActive;
            if (!waveActive)
            {
                mineButton.onClick.AddListener(() =>
                {
                    if (GameManager.Instance != null && GameManager.Instance.isWaveActive) return;
                    isInCaveArea = true;
                    MoveCameraToCave();
                    UpdateMineButton();
                    UpdateShopContent();
                    GameManager.Instance.EnableStartButton(false);
                    if (miningManager != null)
                    {
                        miningManager.gameObject.SetActive(true);
                    }
                });
            }
        }
        else
        {
            mineButton.image.sprite = goBackSprite;
            mineButton.image.color = goBackColor;
            mineButton.interactable = true;
            mineButton.onClick.AddListener(() =>
            {
                isInCaveArea = false;
                MoveCameraToGrass();
                UpdateMineButton();
                UpdateShopContent();
                GameManager.Instance.EnableStartButton(true);
                if (miningManager != null)
                {
                    miningManager.gameObject.SetActive(false);
                }
            });
        }
    }

    private void MoveCameraToGrass()
    {
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(grassCameraPosition.x, grassCameraPosition.y, mainCamera.transform.position.z);
        }
    }

    private void MoveCameraToCave()
    {
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(caveCameraPosition.x, caveCameraPosition.y, mainCamera.transform.position.z);
        }
    }

    private void UpdateShopContent()
    {
        if (lockOverlay != null) lockOverlay.gameObject.SetActive(false);
        if (lockImage != null) lockImage.gameObject.SetActive(false);
        if (unlockText != null) unlockText.gameObject.SetActive(false);

        if (isInCaveArea)
        {
            itemNameText.text = mineTowerName;
            costText.text = $"{mineTowerCopperCost} Copper, {mineTowerIronCost} Iron";
            towerSpriteImage.sprite = mineTowerSprite;
            nextButton.interactable = false;
            previousButton.interactable = false;
            currentIndex = -1;
            buyButton.interactable = true; // Mining Machine always available
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

            if (GameManager.Instance != null && currentIndex >= 0 && currentIndex < unlockWaves.Length)
            {
                int unlockWave = unlockWaves[currentIndex];
                int currentWave = GameManager.Instance.currentWave;
                if (currentWave < unlockWave)
                {
                    buyButton.interactable = false;
                    if (lockOverlay != null) lockOverlay.gameObject.SetActive(true);
                    if (lockImage != null) lockImage.gameObject.SetActive(true);
                    if (unlockText != null)
                    {
                        unlockText.gameObject.SetActive(true);
                        unlockText.text = $"Unlock at Wave {unlockWave}";
                    }
                }
                else
                {
                    buyButton.interactable = true;
                    if (lockOverlay != null) lockOverlay.gameObject.SetActive(false);
                    if (lockImage != null) lockImage.gameObject.SetActive(false);
                    if (unlockText != null) unlockText.gameObject.SetActive(false);
                }
            }
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
        if (openButton != null)
        {
            openButton.gameObject.SetActive(false);
        }
    }

    public void CloseShop()
    {
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
        if (isInCaveArea)
        {
            int copperCost = mineTowerCopperCost;
            int ironCost = mineTowerIronCost;
            if (ResourceManager.Instance.SpendResources(copperCost, ironCost))
            {
                miningPlacement.StartPlacing(copperCost, ironCost);
                CloseShop();
            }
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
        if (!isInCaveArea)
        {
            currentIndex = (currentIndex + 1) % towerPlacement.towerPrefabs.Length;
            UpdateShopContent();
        }
    }

    private void PreviousItem()
    {
        if (!isInCaveArea)
        {
            currentIndex = (currentIndex - 1 + towerPlacement.towerPrefabs.Length) % towerPlacement.towerPrefabs.Length;
            UpdateShopContent();
        }
    }
}