using UnityEngine;

public class ArrowProjectile : Projectile
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 200f;
    [SerializeField] private float trackingRange = 5f;
    [SerializeField] private float damage = 10f;

    private bool rotationLocked;
    private bool isTracking = true;
    private Vector2 lastDirection;
    private float initialAngle;

    new void Update()
    {
        if (base.target == null)
        {
            isTracking = false;
        }

        float distanceToTarget = base.target != null ? Vector2.Distance(transform.position, base.target.position) : float.MaxValue;

        if (isTracking && base.target != null && distanceToTarget > trackingRange)
        {
            isTracking = false;
            lastDirection = (base.target.position - transform.position).normalized;
        }

        Vector2 direction;
        if (isTracking && base.target != null)
        {
            direction = (base.target.position - transform.position).normalized;

            if (!rotationLocked)
            {
                initialAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0, 0, initialAngle);
                rotationLocked = true;
            }
            else
            {
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
            if (lastDirection == Vector2.zero)
            {
                direction = transform.up;
                Destroy(gameObject);
            }
            else
            {
                direction = lastDirection;
            }
        }

        float distanceThisFrame = moveSpeed * Time.deltaTime;
        transform.Translate(direction * distanceThisFrame, Space.World);

        if (isTracking && base.target != null && distanceToTarget <= 0.1f)
        {
            OnHitEnemy(base.target.GetComponent<BaseEnemy>());
            return;
        }

        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        if (viewportPos.x < -0.1f || viewportPos.x > 1.1f || viewportPos.y < -0.1f || viewportPos.y > 1.1f)
        {
            Destroy(gameObject);
        }
    }

    public void SetTarget(Transform _target)
    {
        base.target = _target;
    }

    protected void HitTarget()
    {
        BaseEnemy enemy = base.target != null ? base.target.GetComponent<BaseEnemy>() : null;
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
        Destroy(gameObject);
    }

    protected override void OnHitEnemy(BaseEnemy enemy)
    {
        if (enemy != null)
        {
            HitTarget();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}