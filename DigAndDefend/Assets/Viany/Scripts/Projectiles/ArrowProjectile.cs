using UnityEngine;

public class ArrowProjectile : Projectile
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 200f;
    [SerializeField] private float trackingRange = 5f; // Range within which the arrow tracks the enemy
    [SerializeField] private float damage = 10f; // Damage dealt to enemy

    private bool rotationLocked = false;
    private bool isTracking = true;
    private Vector2 lastDirection;
    private float initialAngle;

    new void Update()
    {
        // Destroy if no target
        if (base.target == null)
        {
            isTracking = false; // Stop tracking if the target is destroyed
        }

        // Calculate distance to target if it exists
        float distanceToTarget = base.target != null ? Vector2.Distance(transform.position, base.target.position) : float.MaxValue;

        // Stop tracking if the target is out of range
        if (isTracking && base.target != null && distanceToTarget > trackingRange)
        {
            isTracking = false;
            lastDirection = (base.target.position - transform.position).normalized; // Store the last direction
        }

        Vector2 direction;
        if (isTracking && base.target != null)
        {
            direction = (base.target.position - transform.position).normalized;

            // Lock initial rotation toward target on the first frame
            if (!rotationLocked)
            {
                initialAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0, 0, initialAngle);
                rotationLocked = true;
            }
            else
            {
                // Calculate desired angle and limit rotation change to ±25 degrees
                float desiredAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                float angleDifference = Mathf.DeltaAngle(initialAngle, desiredAngle);
                float maxAngleChange = 25f;
                if (Mathf.Abs(angleDifference) > maxAngleChange)
                {
                    desiredAngle = initialAngle + Mathf.Sign(angleDifference) * maxAngleChange;
                }
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, desiredAngle), rotateSpeed * Time.deltaTime);
            }
        }
        else
        {
            // If not tracking, use the last direction or destroy if no direction set
            if (lastDirection == Vector2.zero)
            {
                direction = transform.up; // Default to current facing direction if no last direction
                Destroy(gameObject); // Destroy if no valid direction after losing target
            }
            else
            {
                direction = lastDirection;
            }
        }

        float distanceThisFrame = moveSpeed * Time.deltaTime;
        transform.Translate(direction * distanceThisFrame, Space.World);

        // Check if the arrow hits the target while tracking
        if (isTracking && base.target != null && distanceToTarget <= 0.1f)
        {
            OnHitEnemy(base.target.GetComponent<BaseEnemy>());
            return; // Exit early after hitting
        }

        // Check if the arrow is outside the scene boundaries
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        if (viewportPos.x < -0.1f || viewportPos.x > 1.1f || viewportPos.y < -0.1f || viewportPos.y > 1.1f)
        {
            Debug.Log("ArrowProjectile: Arrow left the scene boundaries and was destroyed.");
            Destroy(gameObject);
        }
    }

    public void SetTarget(Transform _target)
    {
        base.target = _target;
    }

    protected void HitTarget()
    {
        if (base.target != null)
        {
            Debug.Log("Arrow hit " + base.target.name); // Safe to access name with null check
        }
        else
        {
            Debug.Log("Arrow hit a destroyed target."); // Log for debugging
        }
        BaseEnemy enemy = base.target != null ? base.target.GetComponent<BaseEnemy>() : null;
        if (enemy != null)
        {
            enemy.TakeDamage(damage); // Apply damage to enemy
        }
        Destroy(gameObject);
    }

    protected override void OnHitEnemy(BaseEnemy enemy)
    {
        if (enemy != null) // Ensure enemy is valid before hitting
        {
            HitTarget();
        }
        else
        {
            Destroy(gameObject); // Destroy if enemy is already null
        }
    }
}