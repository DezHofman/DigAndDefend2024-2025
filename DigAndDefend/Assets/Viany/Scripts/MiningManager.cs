using UnityEngine;

public class MiningManager : MonoBehaviour
{
    [SerializeField] private GameObject miningMachinePrefab;
    [SerializeField] private float resourceGenerationInterval = 10f;
    [SerializeField] private int copperPerInterval = 5;
    [SerializeField] private int ironPerInterval = 3;
    [SerializeField] private int maxHealth = 100; // Maximum health for the tower

    public GameObject MiningMachinePrefab => miningMachinePrefab;

    private void Start()
    {
        gameObject.SetActive(false);
        InvokeRepeating("GenerateResources", resourceGenerationInterval, resourceGenerationInterval);
    }

    void GenerateResources()
    {
        int activeMachines = GameObject.FindGameObjectsWithTag("MiningMachine").Length;
        int totalCopper = activeMachines * copperPerInterval;
        int totalIron = activeMachines * ironPerInterval;
        ResourceManager.Instance.AddCopper(totalCopper);
        ResourceManager.Instance.AddIron(totalIron);
    }

    // Configure a new mining tower with health
    public void ConfigureMiningTower(GameObject tower)
    {
        MiningTower miningTower = tower.AddComponent<MiningTower>();
        miningTower.Initialize(maxHealth);
    }
}