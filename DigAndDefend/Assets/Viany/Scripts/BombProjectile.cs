using UnityEngine;

public class BombProjectile : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab; // Assign in Inspector
    private float explosionRadius;
    private Vector3 targetPosition;
    private float speed = 5f; // Speed at which the bomb moves to the target
    private float explosionDelay = 1f; // 1 second delay before explosion
    private bool hasReachedTarget = false;

    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
        Debug.Log($"BombProjectile: Target position set to {targetPosition}");
    }

    public void SetExplosionRadius(float radius)
    {
        explosionRadius = radius;
    }

    private void Start()
    {
        if (explosionPrefab == null)
        {
            Debug.LogWarning("BombProjectile: Explosion prefab not assigned!");
        }
    }

    private void Update()
    {
        if (!hasReachedTarget)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                hasReachedTarget = true;
                Invoke("Explode", explosionDelay);
                Debug.Log($"BombProjectile: Reached target at {targetPosition}, exploding in {explosionDelay} seconds");
            }
        }
    }

    private void Explode()
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Debug.Log("BombProjectile: Explosion prefab instantiated");
        }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius, LayerMask.GetMask("Enemies"));
        foreach (Collider2D enemy in hitEnemies)
        {
            BaseEnemy baseEnemy = enemy.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                baseEnemy.TakeDamage(20f); // Example damage
                Debug.Log($"BombProjectile: Damaged enemy {enemy.name} for 20 damage");
            }
        }

        Destroy(gameObject);
    }
}