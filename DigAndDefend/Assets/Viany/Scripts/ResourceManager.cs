using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    public int copper = 0;
    public int iron = 0;
    [SerializeField] private TextMeshProUGUI copperText;
    [SerializeField] private TextMeshProUGUI ironText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateResourceUI();
    }

    public bool SpendResources(int copperCost, int ironCost)
    {
        if (copper >= copperCost && iron >= ironCost)
        {
            copper -= copperCost;
            iron -= ironCost;
            UpdateResourceUI();
            return true;
        }
        return false;
    }

    public void AddCopper(int amount)
    {
        copper += amount;
        UpdateResourceUI();
    }

    public void AddIron(int amount)
    {
        iron += amount;
        UpdateResourceUI();
    }

    public void UpdateResourceUI()
    {
        if (copperText != null) copperText.text = "Copper: " + copper;
        if (ironText != null) ironText.text = "Iron: " + iron;
    }
}