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

    private int currentIndex = 0;
    private string[] itemNames = { "Archer Tower", "Bomb Tower", "Slow Tower", "Fire/Poison Tower", "Barricade" };

    private void Start()
    {
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
        openButton.gameObject.SetActive(false); // Hide the open button
    }

    private void CloseShop()
    {
        openedShop.SetActive(false);
        closedShop.SetActive(true);
        openButton.gameObject.SetActive(true); // Show the open button
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