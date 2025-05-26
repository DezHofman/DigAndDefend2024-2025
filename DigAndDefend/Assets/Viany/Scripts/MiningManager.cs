using UnityEngine;

public class MiningManager : MonoBehaviour
{
    public GameObject miningMachinePrefab;
    public Transform[] miningSlots;
    public float resourceGenerationInterval = 10f;
    public int copperPerInterval = 5;
    public int ironPerInterval = 3;

    private void Start()
    {
        InvokeRepeating("GenerateResources", resourceGenerationInterval, resourceGenerationInterval);
        PlaceMiningMachine(0);
    }

    void GenerateResources()
    {
        int activeMachines = 0;
        foreach (Transform slot in miningSlots)
        {
            if (slot.childCount > 0)
            {
                activeMachines++;
            }
        }
        int totalCopper = activeMachines * copperPerInterval;
        int totalIron = activeMachines * ironPerInterval;
        ResourceManager.Instance.AddCopper(totalCopper);
        ResourceManager.Instance.AddIron(totalIron);
    }

    public void PlaceMiningMachine(int slotIndex)
    {
        if (slotIndex < miningSlots.Length && miningSlots[slotIndex].childCount == 0)
        {
            Instantiate(miningMachinePrefab, miningSlots[slotIndex].position, Quaternion.identity, miningSlots[slotIndex]);
        }
    }
}