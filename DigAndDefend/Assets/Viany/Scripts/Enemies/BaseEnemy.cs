using UnityEngine;

public abstract class BaseEnemy : MonoBehaviour
{
    public float speed;
    public int health;
    public int damageToVillage;
    public float damageToBarricade;
    public bool canFly;
    private float originalSpeed;
    private Transform[] waypoints;
    private int currentWaypoint;
    private Healthbar healthbar;
    private bool isAttackingBarricade;
    private EnemySpriteController spriteController;
    private SpriteRenderer spriteRenderer;
    private float slowDuration;
    private bool isSlowed;
    private Vector3 currentVelocity;
    private Color originalColor;
    public GameObject deathEffectPrefab;
    public float deathEffectDuration = 1.0f;
    public Color deathEffectColor = Color.white;

    private void Awake()
    {
        gameObject.tag = "Enemy";
        originalSpeed = speed;
        healthbar = GetComponent<Healthbar>();
        spriteController = GetComponent<EnemySpriteController>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        healthbar.SetInitialHealth(health);
        originalColor = spriteRenderer.color;
        string enemyType = GetEnemyType();
    }

    protected virtual string GetEnemyType()
    {
        return "Unknown";
    }

    public void SetWaypoints(Transform[] pathWaypoints)
    {
        waypoints = pathWaypoints;
    }

    public Transform[] GetWaypoints()
    {
        return waypoints;
    }

    void Update()
    {
        if (isSlowed)
        {
            slowDuration -= Time.deltaTime;
            if (slowDuration <= 0)
            {
                ResetSpeed();
            }
        }

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
        currentVelocity = direction * speed;
        Vector2 movement = currentVelocity * Time.deltaTime;
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
            Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.5f, LayerMask.GetMask("Barricades"));
            if (hit != null)
            {
                Barricade barricade = hit.GetComponent<Barricade>();
                if (barricade != null)
                {
                    isAttackingBarricade = true;
                    barricade.TakeDamage(damageToBarricade);
                    speed = 0f;
                    Invoke("ResumeMovement", 0.5f);
                }
            }
        }

        if (spriteRenderer != null)
        {
            string enemyType = GetEnemyType();
        }
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
        health -= (int)Mathf.Max(1, Mathf.Floor(amount));
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashOnOff());
        }
        if (healthbar != null)
        {
            healthbar.UpdateHealth(health);
        }
        if (health <= 0)
        {
            OnDeath();
        }
    }

    private System.Collections.IEnumerator FlashOnOff()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.enabled = true;
        }
    }

    protected virtual void OnDeath()
    {
        string enemyType = GetEnemyType();
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            SpriteRenderer effectRenderer = effect.GetComponent<SpriteRenderer>();
            if (effectRenderer != null)
            {
                effectRenderer.color = deathEffectColor;
            }
            Destroy(effect, deathEffectDuration);
        }

        Destroy(gameObject);
    }

    public virtual void ApplySlow(float slowFactor, float duration)
    {
        slowFactor = Mathf.Clamp01(slowFactor);
        if (isSlowed)
        {
            slowDuration = Mathf.Max(slowDuration, duration);
        }
        else
        {
            originalSpeed = speed;
            speed *= (1f - slowFactor);
            slowDuration = duration;
            isSlowed = true;
        }
    }

    void ResetSpeed()
    {
        speed = originalSpeed;
        isSlowed = false;
        slowDuration = 0f;
    }

    public virtual void ApplyDoT(float damagePerSecond, float duration)
    {
        StartCoroutine(ApplyDoTOverTime(damagePerSecond, duration));
    }

    private System.Collections.IEnumerator ApplyDoTOverTime(float damagePerSecond, float duration)
    {
        float elapsed = 0f;
        float delay = 0.5f;
        float totalDamage = 0f;
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(delay);
            float damageThisTick = damagePerSecond * delay;
            totalDamage += damageThisTick;
            TakeDamage(damageThisTick);
            elapsed += delay;
        }
    }

    public Vector3 GetCurrentVelocity()
    {
        return currentVelocity;
    }
}