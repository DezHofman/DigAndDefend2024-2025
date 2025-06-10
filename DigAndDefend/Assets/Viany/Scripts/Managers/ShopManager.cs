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

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("ShopManager: Main Camera not found!");
            }
        }

        if (Time.timeScale != 1f)
        {
            Debug.LogWarning("ShopManager: Time.timeScale was not 1, resetting to 1.");
            Time.timeScale = 1f;
        }

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            Debug.LogWarning("ShopManager: No EventSystem found in scene! Adding one.");
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
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
        if (isInCaveArea)
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