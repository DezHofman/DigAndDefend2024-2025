using UnityEngine;

public class ArrowProjectile : Projectile
{
    private bool hasHitGround = false;
    private bool hasHitEnemy = false;

    public new void Initialize(Tower tower, Transform newTarget) // Add 'new' to hide base method
    {
        base.Initialize(tower, newTarget);
        if (newTarget != null)
        {
            BaseEnemy targetEnemy = newTarget.GetComponent<BaseEnemy>();
            if (targetEnemy != null)
            {
                // Predict where the enemy will be based on its velocity and arrow speed
                Vector3 enemyPosition = newTarget.position;
                Vector3 enemyVelocity = targetEnemy.GetCurrentVelocity();
                float timeToHit = Vector3.Distance(transform.position, enemyPosition) / speed;
                Vector3 predictedPosition = enemyPosition + (enemyVelocity * timeToHit);
                Debug.Log($"ArrowProjectile: Predicted enemy position at {predictedPosition}, time to hit: {timeToHit}");

                // Set velocity in the base class
                velocity = (predictedPosition - transform.position).normalized * speed;
                Debug.Log($"ArrowProjectile: Initial velocity: {velocity}");
            }
            else
            {
                velocity = (newTarget.position - transform.position).normalized * speed;
                Debug.Log($"ArrowProjectile: Default velocity: {velocity}");
            }
        }
        else
        {
            Debug.LogWarning("ArrowProjectile: No target assigned!");
            Destroy(gameObject);
        }
    }

    protected override void Update()
    {
        if (hasHitGround || hasHitEnemy || target == null)
        {
            Debug.Log("ArrowProjectile: Destroying arrow due to hit or lost target");
            Destroy(gameObject);
            return;
        }

        Move();
    }

    protected override void Move()
    {
        if (hasHitGround || hasHitEnemy) return;

        transform.position += velocity * Time.deltaTime;

        // Dynamically rotate the arrow to point toward the target
        if (target != null)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f; // -90 to align arrow tip
            transform.rotation = Quaternion.Euler(0, 0, angle);
            Debug.Log($"ArrowProjectile: Rotating to face target at angle {angle}");
        }

        Debug.Log($"ArrowProjectile: Position: {transform.position}, velocity: {velocity}");
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHitEnemy) return;

        if (other.CompareTag("Enemy"))
        {
            BaseEnemy enemy = other.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                hasHitEnemy = true;
                OnHitEnemy(enemy);
                Debug.Log($"ArrowProjectile: Hit enemy {enemy.name} at {transform.position}");
                Destroy(gameObject, 0f);
            }
        }
        else if (other.CompareTag("Ground"))
        {
            hasHitGround = true;
            Debug.Log($"ArrowProjectile: Hit ground at {transform.position}");
            Destroy(gameObject, 0f);
        }
    }

    protected override void OnHitEnemy(BaseEnemy enemy)
    {
        if (parentTower != null)
        {
            enemy.TakeDamage(parentTower.attackDamage);
            Debug.Log($"ArrowProjectile: Dealt {parentTower.attackDamage} damage to enemy {enemy.name}");
        }
        else
        {
            Debug.LogWarning("ArrowProjectile: Parent tower not set, using default damage");
            enemy.TakeDamage(10f); // Fallback damage
        }
    }
}