using UnityEngine;
using UnityEngine.Tilemaps;

public class ArrowProjectile : Projectile
{
    private BoxCollider2D targetCollider;
    private Vector3 velocity;
    private bool hasHitGround = false;
    private Tilemap pathTilemap;
    private Vector3 fixedShotDirection;

    public void SetLastShotDirection(Vector3 direction)
    {
        fixedShotDirection = direction.normalized;
        Debug.Log($"ArrowProjectile: Fixed shot direction set to {fixedShotDirection}");
    }

    protected override void Start()
    {
        base.Start();
        GameObject pathObject = GameObject.Find("PATH");
        if (pathObject != null)
        {
            pathTilemap = pathObject.GetComponent<Tilemap>();
            if (pathTilemap == null)
            {
                Debug.LogWarning("ArrowProjectile: Tilemap component not found on GameObject 'PATH'!");
            }
        }
        else
        {
            Debug.LogWarning("ArrowProjectile: Could not find GameObject named 'PATH' in the scene!");
        }

        if (target != null)
        {
            targetCollider = target.GetComponent<BoxCollider2D>();
            if (targetCollider != null)
            {
                Vector3 direction = (target.position - transform.position).normalized;
                float distanceToEdge = Vector3.Distance(transform.position, targetCollider.ClosestPoint(transform.position));
                velocity = direction * speed;
            }
            else
            {
                velocity = (targetPosition - transform.position).normalized * speed;
            }
            Debug.Log($"ArrowProjectile Start: Target exists, initial velocity: {velocity}, speed: {speed}");
        }
        else
        {
            Debug.LogWarning("ArrowProjectile Start: No target assigned!");
            Destroy(gameObject);
        }
    }

    protected override void Update()
    {
        if (hasHitGround || target == null)
        {
            Destroy(gameObject);
            return;
        }

        if (target != null && target.gameObject.activeInHierarchy)
        {
            targetCollider = target.GetComponent<BoxCollider2D>();
            if (targetCollider != null)
            {
                targetPosition = targetCollider.ClosestPoint(transform.position);
            }
            velocity = (targetPosition - transform.position).normalized * speed;
            Debug.Log($"ArrowProjectile Update: Target active, targetPosition: {targetPosition}, velocity: {velocity}");
        }

        Move();
    }

    protected override void Move()
    {
        if (hasHitGround) return;

        transform.position += velocity * Time.deltaTime;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(fixedShotDirection.y, fixedShotDirection.x) * Mathf.Rad2Deg - 90f);
        Debug.Log($"ArrowProjectile Move: Position: {transform.position}, velocity: {velocity}");
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && other is BoxCollider2D)
        {
            Debug.Log("ArrowProjectile: Hit enemy at " + transform.position);
            OnHit();
            Destroy(gameObject); // Immediate destruction on enemy hit
        }
        else if (other.CompareTag("Ground"))
        {
            hasHitGround = true;
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