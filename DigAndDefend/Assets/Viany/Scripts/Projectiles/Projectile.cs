using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected Transform target;
    protected float speed = 10f;
    protected Tower parentTower;
    protected bool hasHit = false;
    protected Vector3 velocity; // Ensure this is not [SerializeField]

    public void Initialize(Tower tower, Transform newTarget)
    {
        parentTower = tower;
        target = newTarget;
        transform.SetParent(null); // Ensure projectile isn’t parented
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
            Debug.Log($"Projectile: Collider set as trigger, type: {collider.GetType()}");
        }
        else
        {
            Debug.LogError("Projectile: No Collider2D found on projectile!");
        }
    }

    protected virtual void Update()
    {
        if (hasHit) return;

        if (target != null && target.gameObject.activeInHierarchy)
        {
            Move();
        }
        else
        {
            Debug.Log("Projectile: Target lost, destroying projectile");
            Destroy(gameObject);
        }
    }

    protected virtual void Move()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        velocity = direction * speed; // Use velocity for consistency
        transform.position += velocity * Time.deltaTime;
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        if (other.CompareTag("Enemy"))
        {
            BaseEnemy enemy = other.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                hasHit = true;
                OnHitEnemy(enemy);
                Debug.Log($"Projectile: Hit enemy {enemy.name} at {transform.position}");
                Destroy(gameObject, 0f); // Immediate destruction
            }
        }
    }

    protected abstract void OnHitEnemy(BaseEnemy enemy);
}