using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image towerSpriteImage; // Reference to UI Image for tower sprite
    [SerializeField] private Sprite[] towerSprites; // Array of tower sprites, assigned in Inspector

    private int currentIndex = 0;
    private string[] itemNames = { "Archer Tower", "Bomb Tower", "Slow Tower", "Fire/Poison Tower", "Barricade" };

    private void Start()
    {
        if (towerSprites.Length != towerPlacement.towerPrefabs.Length)
        {
            Debug.LogWarning("ShopManager: towerSprites array length does not match towerPrefabs length!");
        }

        openedShop.SetActive(false);
        openButton.onClick.AddListener(OpenShop);
        closeButton.onClick.AddListener(CloseShop);
        buyButton.onClick.AddListener(BuyCurrentTower);
        nextButton.onClick.AddListener(NextItem);
        previousButton.onClick.AddListener(PreviousItem);
        UpdateDisplay();
    }

    private void OpenShop()
    {
        currentIndex = 0;
        UpdateDisplay();
        openedShop.SetActive(true);
        closedShop.SetActive(false);
        openButton.gameObject.SetActive(false);
    }

    private void CloseShop()
    {
        openedShop.SetActive(false);
        closedShop.SetActive(true);
        openButton.gameObject.SetActive(true);
    }

    private void NextItem()
    {
        currentIndex = (currentIndex + 1) % towerPlacement.towerPrefabs.Length;
        UpdateDisplay();
    }

    private void PreviousItem()
    {
        currentIndex = (currentIndex - 1 + towerPlacement.towerPrefabs.Length) % towerPlacement.towerPrefabs.Length;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        itemNameText.text = itemNames[currentIndex];
        int copperCost = towerPlacement.copperCosts[currentIndex];
        int ironCost = towerPlacement.ironCosts[currentIndex];
        costText.text = $"{copperCost} Copper, {ironCost} Iron";

        // Update the tower sprite in the shop UI
        if (towerSpriteImage != null && currentIndex < towerSprites.Length)
        {
            towerSpriteImage.sprite = towerSprites[currentIndex];
            Debug.Log($"ShopManager: Displaying sprite for {itemNames[currentIndex]}");
        }
        else
        {
            Debug.LogWarning("ShopManager: towerSpriteImage not assigned or invalid sprite index!");
        }
    }

    private void BuyCurrentTower()
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
                Debug.Log("Not enough resources! Need " + copperCost + " Copper and " + ironCost + " Iron.");
            }
        }
    }
}