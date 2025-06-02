using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour
{
    public float speed;
    public int health;
    public int damageToVillage;
    public float damageToBarricade;
    public bool canFly = false;
    private float originalSpeed;
    private Transform[] waypoints;
    private int currentWaypoint = 0;
    private Healthbar healthbar;
    private bool isAttackingBarricade = false;
    private EnemySpriteController spriteController;

    private void Awake()
    {
        gameObject.tag = "Enemy";
        originalSpeed = speed;
        healthbar = GetComponent<Healthbar>();
        spriteController = GetComponent<EnemySpriteController>();
        if (healthbar != null)
        {
            healthbar.SetInitialHealth(health);
        }

        string enemyType = GetEnemyType();
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = enemyType;
        }
        EnemySortingManager.AssignSortingOrder(gameObject, enemyType, Vector2.zero);
    }

    protected virtual string GetEnemyType()
    {
        return "Unknown";
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
        Vector2 movement = direction * speed * Time.deltaTime;
        transform.position += (Vector3)movement;

        if (spriteController != null)
        {
            spriteController.UpdateSpriteDirection(direction);
        }

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentWaypoint++;
        }

        if (!canFly && !isAttackingBarricade)
        {
            Collider2D[] barricadeColliders = Physics2D.OverlapCircleAll(transform.position, 0.5f, LayerMask.GetMask("Barricades"));
            if (barricadeColliders.Length > 0)
            {
                Barricade barricade = barricadeColliders[0].GetComponent<Barricade>();
                if (barricade != null)
                {
                    isAttackingBarricade = true;
                    barricade.TakeDamage(damageToBarricade);
                    speed = 0f;
                    Invoke("ResumeMovement", 0.5f);
                }
            }
        }

        string enemyType = GetEnemyType();
        EnemySortingManager.ReassignOrders(enemyType, direction);
    }

    void ResumeMovement()
    {
        speed = originalSpeed;
        isAttackingBarricade = false;
    }

    void AttackVillage()
    {
        GameManager.Instance.TakeDamage(damageToVillage);
        Destroy(gameObject);
    }

    public virtual void TakeDamage(float amount)
    {
        Debug.Log("Taking damage: " + amount + ", Current health: " + health);
        health -= (int)amount;
        if (healthbar != null)
        {
            healthbar.UpdateHealth(health);
        }
        if (health <= 0)
        {
            OnDeath();
        }
    }

    protected virtual void OnDeath()
    {
        string enemyType = GetEnemyType();
        EnemySortingManager.ReleaseSortingOrder(gameObject, enemyType);
        Destroy(gameObject);
    }

    public virtual void ApplySlow(float slowFactor)
    {
        speed *= (1 - slowFactor);
        Invoke("ResetSpeed", 2f);
    }

    void ResetSpeed()
    {
        speed = originalSpeed;
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