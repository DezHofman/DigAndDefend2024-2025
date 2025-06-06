using UnityEngine;

public class IceCreamProjectile : Projectile
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float slowDuration = 2f;
    [SerializeField] private float slowFactor = 0.5f; // Reduces speed by 50%
    [SerializeField] private float damage = 5f; // Damage dealt to enemy
    private bool hasIceCreamHit = false; // Renamed to avoid conflict with base class

    new void Update() // Added 'new' to hide base Update method
    {
        if (base.target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 direction = (base.target.position - transform.position).normalized;
        float distanceThisFrame = moveSpeed * Time.deltaTime;
        transform.Translate(direction * distanceThisFrame, Space.World);

        // Destroy after traveling a certain distance or hitting
        if (Vector2.Distance(transform.position, base.target.position) > 10f || hasIceCreamHit)
        {
            Destroy(gameObject, 0.1f); // Slight delay to apply effect
        }
    }

    protected void HitTarget()
    {
        Debug.Log("Ice cream hit " + base.target.name);
        BaseEnemy enemy = base.target.GetComponent<BaseEnemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage); // Apply damage to enemy
        }
    }

    protected override void OnHitEnemy(BaseEnemy enemy)
    {
        if (!hasIceCreamHit)
        {
            hasIceCreamHit = true;
            HitTarget();
            // Apply slow effect
            enemy.ApplySlow(slowFactor, slowDuration);
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        BaseEnemy enemy = collision.GetComponent<BaseEnemy>();
        if (enemy != null && base.target != null && collision.gameObject == base.target.gameObject)
        {
            OnHitEnemy(enemy);
        }
    }
}