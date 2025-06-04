using UnityEngine;

public class BombProjectile : Projectile
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionRadius = 1f;
    private float explosionDelay;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool hasFlashed = false;

    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            Debug.Log($"BombProjectile: SpriteRenderer found, original color: {originalColor}");
        }
        else
        {
            Debug.LogWarning("BombProjectile: No SpriteRenderer found on bomb!");
        }
        explosionDelay = lifetime * 0.5f;
        speed = Vector3.Distance(parentTower.transform.position, targetPosition) / (lifetime * 0.5f);
    }

    protected override void Update()
    {
        base.Update();
        if (hasFlashed || spriteRenderer == null) return;

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            float timeSinceReached = Time.time - (lifetime * 0.5f);
            if (timeSinceReached >= explosionDelay - 0.2f)
            {
                spriteRenderer.color = Color.white;
                hasFlashed = true;
                Debug.Log("BombProjectile: Flashing white!");
            }
        }
    }

    protected override void Move()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            Invoke("Explode", explosionDelay);
            speed = 0;
        }
    }

    void Explode()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor; // Reset color before destroying
        }
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D collider in hitColliders)
        {
            BaseEnemy enemy = collider.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(parentTower.attackDamage);
            }
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPosition, explosionRadius);
    }
}