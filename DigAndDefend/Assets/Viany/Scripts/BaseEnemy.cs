using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour
{
    public float speed = 2f;
    public int health = 50;
    public int damageToVillage = 10;
    public bool canFly = false;
    private Transform[] waypoints;
    private int currentWaypoint = 0;

    private void Awake()
    {
        gameObject.tag = "Enemy";
    }

    public void SetWaypoints(Transform[] pathWaypoints)
    {
        waypoints = pathWaypoints;
    }

    void Update()
    {
        if (currentWaypoint < waypoints.Length)
        {
            MoveTowardsWaypoint();
        }
        else
        {
            AttackVillage();
        }
    }

    void MoveTowardsWaypoint()
    {
        Vector2 targetPosition = waypoints[currentWaypoint].position;
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentWaypoint++;
        }

        if (!canFly)
        {
            Collider2D barricade = Physics2D.OverlapCircle(transform.position, 0.5f, LayerMask.GetMask("Barricades"));
            if (barricade != null)
            {
                health -= 10;
                Destroy(barricade.gameObject);
                if (health <= 0) OnDeath();
            }
        }
    }

    void AttackVillage()
    {
        GameManager.Instance.TakeDamage(damageToVillage);
        Destroy(gameObject);
    }

    public virtual void TakeDamage(float amount)
    {
        health -= (int)amount;
        if (health <= 0)
        {
            OnDeath();
        }
    }

    protected virtual void OnDeath()
    {
        Destroy(gameObject);
    }

    public virtual void ApplySlow(float slowFactor)
    {
        speed *= (1 - slowFactor);
    }

    public virtual void ApplyDoT(float damagePerSecond, float duration)
    {
        StartCoroutine(ApplyDoTOverTime(damagePerSecond, duration));
    }

    private System.Collections.IEnumerator ApplyDoTOverTime(float damagePerSecond, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            TakeDamage(damagePerSecond * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}