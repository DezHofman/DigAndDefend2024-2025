using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected Transform target;
    protected float speed = 10f;
    protected Tower parentTower;
    protected bool hasHit;
    protected Vector3 velocity;

    public void Initialize(Tower tower, Transform newTarget)
    {
        parentTower = tower;
        target = newTarget;
        transform.SetParent(null);
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
            Destroy(gameObject);
        }
    }

    protected virtual void Move()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        velocity = direction * speed;
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
                Destroy(gameObject);
            }
        }
    }

    protected abstract void OnHitEnemy(BaseEnemy enemy);
}