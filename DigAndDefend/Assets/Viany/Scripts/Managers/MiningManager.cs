using UnityEngine;
using System.Collections;

public class MiningManager : MonoBehaviour
{
    [SerializeField] private GameObject miningMachinePrefab;
    [SerializeField] private float resourceGenerationInterval = 10f;
    [SerializeField] private int copperPerInterval = 5;
    [SerializeField] private int ironPerInterval = 3;
    [SerializeField] private int maxHealth = 100;

    public GameObject MiningMachinePrefab => miningMachinePrefab;

    private void Start()
    {
        StartCoroutine(ResourceGenerationLoop());
        InvokeRepeating("GenerateResources", resourceGenerationInterval, resourceGenerationInterval);
    }

    private IEnumerator ResourceGenerationLoop()
    {
        yield return new WaitForSeconds(resourceGenerationInterval);
        yield return new WaitForSeconds(resourceGenerationInterval);

        while (true)
        {
            GenerateResources();
            yield return new WaitForSeconds(resourceGenerationInterval);
        }
    }

    void GenerateResources()
    {
        int activeMachines = GameObject.FindGameObjectsWithTag("MiningMachine").Length;
        int totalCopper = activeMachines * copperPerInterval;
        int totalIron = activeMachines * ironPerInterval;
        ResourceManager.Instance.AddCopper(totalCopper);
        ResourceManager.Instance.AddIron(totalIron);
    }

    public void ConfigureMiningTower(GameObject tower)
    {
        MiningTower miningTower = tower.GetComponent<MiningTower>();
        miningTower.Initialize(maxHealth);
    }
}
