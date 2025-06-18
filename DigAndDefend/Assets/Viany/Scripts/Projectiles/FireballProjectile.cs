using UnityEngine;

public class FireballProjectile : Projectile
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 200f;
    [SerializeField] private float trackingRange = 5f;
    [SerializeField] private float damage = 10f;
    [SerializeField] public float dotDamagePerSecond = 5f;
    [SerializeField] public float dotDuration = 3f;

    private bool rotationLocked;
    private bool isTracking = true;
    private Vector2 lastDirection;
    private float initialAngle;

    new void Update()
    {
        Vector2 direction = Vector2.zero;
        if (target == null)
        {
            isTracking = false;
        }

        float distanceToTarget = target != null ? Vector2.Distance(transform.position, target.position) : float.MaxValue;
        if (isTracking)
        {
            if (distanceToTarget > trackingRange)
            {
                isTracking = false;
                lastDirection = (target.position - transform.position).normalized;
            }

            direction = (target.position - transform.position).normalized;

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
        else if (!target)
        {
            if (lastDirection == Vector2.zero)
            {
                direction = transform.up;
            }
            else
            {
                direction = lastDirection;
            }
        }

        float distanceThisFrame = moveSpeed * Time.deltaTime;
        transform.Translate(direction * distanceThisFrame, Space.World);

        if (isTracking && target != null && distanceToTarget <= 1f)
        {
            OnHitEnemy(target.GetComponent<BaseEnemy>());
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
        target = _target;
    }

    protected void HitTarget()
    {
        BaseEnemy enemy = target.GetComponent<BaseEnemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            enemy.ApplyDoT(dotDamagePerSecond, dotDuration);
        }
        Destroy(gameObject);
    }

    protected override void OnHitEnemy(BaseEnemy enemy)
    {
        HitTarget();
    }

    new void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            BaseEnemy enemy = other.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                OnHitEnemy(enemy);
            }
        }
    }
}