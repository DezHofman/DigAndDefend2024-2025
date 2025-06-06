using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class MiningManager : MonoBehaviour
{
    [SerializeField] public GameObject miningMachinePrefab;
    [SerializeField] private Transform[] miningSlots;
    [SerializeField] private float resourceGenerationInterval = 10f;
    [SerializeField] private int copperPerInterval = 5;
    [SerializeField] private int ironPerInterval = 3;

    public Transform[] MiningSlots => miningSlots;

    private void Start()
    {
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
}