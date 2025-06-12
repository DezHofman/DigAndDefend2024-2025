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
    private int currentWaypoint = 0;
    private Healthbar healthbar;
    private bool isAttackingBarricade = false;
    private EnemySpriteController spriteController;
    private SpriteRenderer spriteRenderer;
    private float slowDuration;
    private bool isSlowed = false;
    private Vector3 currentVelocity; // Track velocity
    private Color originalColor;

    [Header("Death Effect")]
    public GameObject deathEffectPrefab; // Assign the prefab in the inspector
    public float deathEffectDuration = 1.0f; // Adjustable duration in seconds
    public Color deathEffectColor = Color.white; // Adjustable color in inspector

    private void Awake()
    {
        gameObject.tag = "Enemy";
        originalSpeed = speed;
        healthbar = GetComponent<Healthbar>();
        spriteController = GetComponent<EnemySpriteController>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (healthbar != null)
        {
            healthbar.SetInitialHealth(health);
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color; // Store original color
            string enemyType = GetEnemyType();
            UpdateSortingLayer(enemyType, Vector2.zero); // Initial update
            EnemySortingManager.AssignSortingOrder(gameObject, enemyType);
        }
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
        currentVelocity = direction * speed; // Update velocity
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

        if (spriteRenderer != null)
        {
            string enemyType = GetEnemyType();
            UpdateSortingLayer(enemyType, direction);
        }
    }

    void UpdateSortingLayer(string enemyType, Vector2 direction)
    {
        string layerSuffix;
        if (direction.y > 0.7f)
        {
            layerSuffix = "Up";
        }
        else if (direction.y < -0.7f)
        {
            layerSuffix = "Down";
        }
        else
        {
            layerSuffix = "RightLeft";
        }

        string layerName = "Enemy_" + layerSuffix;
        spriteRenderer.sortingLayerName = layerName;

        var enemies = EnemySortingManager.GetActiveEnemies(enemyType);
        if (enemies == null) return;
        int enemyCount = enemies.Count;
        int myIndex = enemies.IndexOf(gameObject);
        if (myIndex < 0) myIndex = enemyCount;

        int baseOrder, maxOrder;
        bool reverseOrder = false;
        switch (layerSuffix)
        {
            case "Up":
                baseOrder = 500;
                maxOrder = 999;
                reverseOrder = false;
                break;
            case "RightLeft":
                baseOrder = 0;
                maxOrder = 499;
                reverseOrder = true;
                break;
            case "Down":
                baseOrder = 1000;
                maxOrder = 1500;
                reverseOrder = true;
                break;
            default:
                baseOrder = 0;
                maxOrder = 499;
                reverseOrder = true;
                break;
        }

        int order;
        if (enemyCount > 1)
        {
            int orderRange = maxOrder - baseOrder;
            if (reverseOrder)
            {
                order = maxOrder - (myIndex * (orderRange / (enemyCount - 1)));
            }
            else
            {
                order = baseOrder + (myIndex * (orderRange / (enemyCount - 1)));
            }
        }
        else
        {
            order = reverseOrder ? maxOrder : baseOrder;
        }

        spriteRenderer.sortingOrder = Mathf.Clamp(order, baseOrder, maxOrder);

        Debug.Log($"Set {gameObject.name} to layer: {layerName}, order: {spriteRenderer.sortingOrder}, index: {myIndex}, count: {enemyCount}, direction: {direction}, frame: {Time.frameCount}");
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
        health -= (int)Mathf.Max(1, Mathf.Floor(amount)); // Floor to nearest integer, minimum 1
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
            spriteRenderer.enabled = false; // Turn off rendering
            yield return new WaitForSeconds(0.1f); // Off for 0.1 seconds
            spriteRenderer.enabled = true; // Turn on rendering
            // Ends after one flash
        }
    }

    protected virtual void OnDeath()
    {
        string enemyType = GetEnemyType();
        EnemySortingManager.ReleaseSortingOrder(gameObject, enemyType);

        // Spawn death effect if prefab is assigned
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            SpriteRenderer effectRenderer = effect.GetComponent<SpriteRenderer>();
            if (effectRenderer != null)
            {
                effectRenderer.color = deathEffectColor; // Set the color
            }

            // Destroy effect after duration
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
            Debug.Log($"{gameObject.name} is slowed by {slowFactor} for {duration} seconds.");
        }
    }

    void ResetSpeed()
    {
        speed = originalSpeed;
        isSlowed = false;
        slowDuration = 0f;
        Debug.Log($"{gameObject.name} speed reset to {speed}.");
    }

    public virtual void ApplyDoT(float damagePerSecond, float duration)
    {
        StartCoroutine(ApplyDoTOverTime(damagePerSecond, duration));
    }

    private System.Collections.IEnumerator ApplyDoTOverTime(float damagePerSecond, float duration)
    {
        float elapsed = 0f;
        float delay = 0.5f; // Add 0.5-second delay between ticks
        float totalDamage = 0f;
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(delay);
            float damageThisTick = damagePerSecond * delay; // Calculate damage for this interval
            totalDamage += damageThisTick;
            TakeDamage(damageThisTick);
            Debug.Log($"DoT tick: {damageThisTick}, Total Damage: {totalDamage}, Health: {health}");
            elapsed += delay;
        }
    }

    public Vector3 GetCurrentVelocity()
    {
        return currentVelocity;
    }
}