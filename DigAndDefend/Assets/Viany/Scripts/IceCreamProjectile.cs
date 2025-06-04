using UnityEngine;

public class IceCreamProjectile : Projectile
{
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float boomerangDistance = 2f;
    private Vector3 initialPosition;
    private float journeyLength;
    private float startTime;

    protected override void Start()
    {
        base.Start();
        initialPosition = transform.position;
        if (target != null)
        {
            Vector3 offset = Random.insideUnitCircle * boomerangDistance;
            targetPosition = target.position + offset;
        }
        journeyLength = Vector3.Distance(initialPosition, targetPosition);
        startTime = Time.time;
        speed = journeyLength / (lifetime * 0.8f);
    }

    protected override void Move()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        float distCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distCovered / journeyLength;
        transform.position = Vector3.Lerp(initialPosition, targetPosition, fractionOfJourney);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
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
                Debug.Log($"Applying slow to {enemy.gameObject.name}: 0.5f for 2f");
                enemy.ApplySlow(0.5f, 2f);
            }
        }
    }
}