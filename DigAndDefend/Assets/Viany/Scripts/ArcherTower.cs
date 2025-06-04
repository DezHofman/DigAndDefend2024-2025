using UnityEngine;
using UnityEngine.Tilemaps;

public class ArcherTower : Tower
{
    [SerializeField] private float defaultShootDistance = 5f;
    private Tilemap pathTilemap;
    private bool hasFiredDefault = false;
    private Vector3 lastShotDirection = Vector3.right;

    protected override void Start()
    {
        base.Start();
        GameObject pathObject = GameObject.Find("PATH");
        if (pathObject != null)
        {
            pathTilemap = pathObject.GetComponent<Tilemap>();
            if (pathTilemap == null)
            {
                Debug.LogWarning("ArcherTower: Tilemap component not found on GameObject 'PATH'!");
            }
            else
            {
                Debug.Log($"ArcherTower: Found Tilemap 'PATH' at {pathObject.transform.position}");
            }
        }
        else
        {
            Debug.LogWarning("ArcherTower: Could not find GameObject named 'PATH' in the scene!");
        }
    }

    protected override void HandleAttack(Collider2D[] enemies)
    {
        Transform target = null;
        Vector3 targetPosition = transform.position;

        if (enemies.Length > 0)
        {
            Collider2D closestEnemy = null;
            float closestDistance = float.MaxValue;
            foreach (Collider2D enemy in enemies)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }

            if (closestEnemy != null)
            {
                target = closestEnemy.transform;
                targetPosition = closestEnemy.transform.position;
                lastShotDirection = (targetPosition - transform.position).normalized;
                hasFiredDefault = false;
                Debug.Log($"ArcherTower: Targeting enemy at {targetPosition}, lastShotDirection: {lastShotDirection}");
            }
        }

        if (target != null)
        {
            GameObject projectileObj = Instantiate(projectilePrefab, transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity);
            ArrowProjectile proj = projectileObj.GetComponent<ArrowProjectile>();
            if (proj != null)
            {
                proj.Initialize(this, target, targetPosition, 1f / attackSpeed);
                proj.SetLastShotDirection(lastShotDirection);
            }
        }
    }
}