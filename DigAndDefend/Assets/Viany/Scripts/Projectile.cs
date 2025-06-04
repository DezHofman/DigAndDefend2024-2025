using UnityEngine;

public class Projectile : MonoBehaviour
{
    protected float speed = 10f;
    protected float lifetime;
    protected Transform target;
    protected Vector3 targetPosition;
    protected Tower parentTower;

    protected virtual void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Tower tower, Transform target, Vector3 position, float attackInterval)
    {
        parentTower = tower;
        this.target = target;
        targetPosition = position;
        lifetime = attackInterval;
    }

    protected virtual void Update()
    {
        if (target != null)
        {
            targetPosition = target.position;
        }
        Move();
    }

    protected virtual void Move() { }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            OnHit();
            Destroy(gameObject);
        }
    }

    protected virtual void OnHit() { }
}