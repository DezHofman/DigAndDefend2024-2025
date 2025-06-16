using UnityEngine;

public class ZLayering : MonoBehaviour
{
    private BaseEnemy enemy;
    private bool isStatic = false;
    private const float BASE_Y = 1.5f;
    private const float FLYING_Z = -10f;
    private const float Z_OFFSET = 0.001f; // Small offset for X-axis order
    private static int globalOrderCounter = 0; // Tracks placement/spawn order
    [SerializeField] private int order; // Instance-specific order

    private void Start()
    {
        enemy = GetComponent<BaseEnemy>();
        isStatic = GetComponent<Tower>() != null || GetComponent<Barricade>() != null;
        if (order == 0)
        {
            order = ++globalOrderCounter;
            Debug.Log($"ZLayering {gameObject.name}: Assigned order={order}, isFlying={(enemy != null ? enemy.canFly.ToString() : "N/A")}", this);
        }
        if (isStatic)
        {
            UpdateZPosition();
        }
    }

    private void Update()
    {
        if (!isStatic)
        {
            UpdateZPosition();
        }
    }

    private void UpdateZPosition()
    {
        float yPos = transform.position.y;
        float zPos;

        if (enemy != null && enemy.canFly)
        {
            zPos = FLYING_Z; // Flying enemies always in front
        }
        else
        {
            float baseZ = yPos - BASE_Y; // Y-axis layering
            float orderOffset = isStatic ? -order * Z_OFFSET : order * Z_OFFSET;
            zPos = baseZ + orderOffset;
        }

        transform.position = new Vector3(transform.position.x, yPos, zPos);
        // Debug.Log($"ZLayering {gameObject.name}: Z={zPos}, Y={yPos}, order={order}{(enemy != null && enemy.isFlying ? " (Flying)" : "")}", this);
    }

    public void SetOrder(int newOrder)
    {
        order = newOrder;
        Debug.Log($"ZLayering {gameObject.name}: Set order={order}", this);
    }
}