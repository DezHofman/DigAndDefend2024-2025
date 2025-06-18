using UnityEngine;

public class IceCreamProjectile : Projectile
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float slowDuration = 2f;
    [SerializeField] private float slowFactor = 0.5f;
    [SerializeField] private float damage = 5f;
    [SerializeField] private float spinSpeed = 360f;
    private bool hasIceCreamHit;

    new void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 direction = (target.position - transform.position).normalized;
        float distanceThisFrame = moveSpeed * Time.deltaTime;
        transform.Translate(direction * distanceThisFrame, Space.World);

        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) > 10f || hasIceCreamHit)
        {
            Destroy(gameObject);
        }
    }

    protected void HitTarget()
    {
        BaseEnemy enemy = target.GetComponent<BaseEnemy>();
        enemy.TakeDamage(damage);
    }

    protected override void OnHitEnemy(BaseEnemy enemy)
    {
        hasIceCreamHit = true;
        HitTarget();
        enemy.ApplySlow(slowFactor, slowDuration);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        BaseEnemy enemy = collision.GetComponent<BaseEnemy>();
        if (enemy != null && target != null && collision.gameObject == target.gameObject)
        {
            OnHitEnemy(enemy);
        }
    }
}