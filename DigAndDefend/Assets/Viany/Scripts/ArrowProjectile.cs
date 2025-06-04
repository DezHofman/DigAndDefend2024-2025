using UnityEngine;

public class ArrowProjectile : Projectile
{
    private BoxCollider2D targetCollider;

    protected override void Start()
    {
        base.Start();
        if (target != null)
        {
            targetCollider = target.GetComponent<BoxCollider2D>();
            if (targetCollider != null)
            {
                Vector3 direction = (target.position - transform.position).normalized;
                Vector3 closestPoint = targetCollider.ClosestPoint(transform.position);
                float distanceToEdge = Vector3.Distance(transform.position, closestPoint);
                speed = distanceToEdge / lifetime;
                targetPosition = closestPoint;
            }
            else
            {
                speed = Vector3.Distance(transform.position, targetPosition) / lifetime;
            }
        }
    }

    protected override void Move()
    {
        if (target != null && targetCollider != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            Vector3 direction = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && other is BoxCollider2D)
        {
            OnHit();
            Destroy(gameObject);
        }
    }

    protected override void OnHit()
    {
        if (target != null)
        {
            BaseEnemy enemy = target.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(parentTower.attackDamage);
            }
        }
    }
}