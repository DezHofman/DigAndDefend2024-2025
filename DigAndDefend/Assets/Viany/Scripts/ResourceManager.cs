using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    public int copper = 0;
    public int iron = 0;
    public TMP_Text copperText;
    public TMP_Text ironText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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

    void UpdateResourceUI()
    {
        copperText.text = "Copper: " + copper;
        ironText.text = "Iron: " + iron;
    }
}