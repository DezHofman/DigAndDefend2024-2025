using UnityEngine;

public abstract class Tower : MonoBehaviour
{
    public float attackRange;
    public float attackSpeed;
    public float attackDamage;
    public float rangeDisplayDuration;
    public GameObject projectilePrefab;
    public GameObject rangeIndicatorPrefab;
    protected float timeSinceLastAttack;
    protected CircleCollider2D attackCollider;
    private GameObject rangeIndicator;
    private float rangeVisibleTime;
    private bool isRangeVisible;
    private SpriteRenderer spriteRenderer;
    private Vector3 initialPosition;
    public float clickAreaRadius;

    private string baseLayer = "Towers_Below"; // Default layer below enemies
    private string aboveLayer = "Towers_Above"; // Layer above enemies

    protected virtual void Start()
    {
        initialPosition = transform.position;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        attackCollider = GetComponent<CircleCollider2D>();
        if (attackCollider != null)
        {
            attackCollider.radius = attackRange;
            attackCollider.isTrigger = true;
        }
        gameObject.tag = "Tower";
        gameObject.layer = LayerMask.NameToLayer("Towers");

        rangeIndicator = Instantiate(rangeIndicatorPrefab, transform.position, Quaternion.identity, transform);
        rangeIndicator.transform.localScale = new Vector3(attackRange * 2, attackRange * 2, 1);
        rangeIndicator.SetActive(false);
        rangeIndicator.layer = LayerMask.NameToLayer("Ignore Raycast");

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = baseLayer; // Start with Towers_Below
        }
        else
        {
            Debug.LogError("Tower missing SpriteRenderer!");
        }
    }

    protected virtual void Update()
    {
        timeSinceLastAttack += Time.deltaTime;
        if (timeSinceLastAttack >= 1f / attackSpeed)
        {
            Attack();
            timeSinceLastAttack = 0f;
        }

        if (isRangeVisible)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 localMousePos = mousePos - new Vector2(transform.position.x, transform.position.y);
            if (localMousePos.magnitude > clickAreaRadius)
            {
                rangeVisibleTime += Time.deltaTime;
                if (rangeVisibleTime >= rangeDisplayDuration)
                {
                    rangeIndicator.SetActive(false);
                    isRangeVisible = false;
                }
            }
            else
            {
                rangeVisibleTime = 0f;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, LayerMask.GetMask("Towers"));
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject == gameObject)
                {
                    Vector2 localMousePos = hit.point - new Vector2(transform.position.x, transform.position.y);
                    if (localMousePos.magnitude <= clickAreaRadius)
                    {
                        if (rangeIndicator != null)
                        {
                            rangeIndicator.SetActive(true);
                            isRangeVisible = true;
                            rangeVisibleTime = 0f;
                        }
                    }
                }
            }
        }
    }

    protected virtual void Attack()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Enemies"));
        if (enemies.Length > 0)
        {
            HandleAttack(enemies);
            // Update tower layer based on nearest enemy
            UpdateTowerLayer(enemies);
        }
        else
        {
            ResetTowerLayer();
        }
    }

    protected abstract void HandleAttack(Collider2D[] enemies);

    private void UpdateTowerLayer(Collider2D[] enemies)
    {
        if (spriteRenderer == null) return;

        // Find the nearest enemy
        Collider2D nearestEnemy = null;
        float minDistance = float.MaxValue;
        foreach (Collider2D enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null)
        {
            float enemyY = nearestEnemy.transform.position.y;
            float towerY = transform.position.y;

            // Switch layer based on enemy Y-position
            // Assume lower Y is in front (adjust if your Y-axis is inverted)
            if (enemyY < towerY)
            {
                // Enemy is in front, tower should be below
                spriteRenderer.sortingLayerName = baseLayer;
            }
            else
            {
                // Enemy is behind, tower should be above
                spriteRenderer.sortingLayerName = aboveLayer;
            }
        }
    }

    private void ResetTowerLayer()
    {
        if (spriteRenderer != null)
        {
            // Reset to base layer when no enemies are near
            spriteRenderer.sortingLayerName = baseLayer;
        }
    }

    private void OnDestroy()
    {
        if (rangeIndicator != null)
        {
            Destroy(rangeIndicator);
        }
    }
}