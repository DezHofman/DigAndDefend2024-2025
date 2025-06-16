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
    private const float LAYER_SWITCH_BUFFER = 0.1f; // Small buffer to prevent rapid toggling

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
            // Removed call to UpdateSortingOrder since it's now handled by UpdateLayerAndOrder
            UpdateLayerAndOrder(); // Initialize layer and sorting order
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

        // Update sorting order and layer continuously
        UpdateLayerAndOrder();
    }

    protected virtual void Attack()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Enemies"));
        if (enemies.Length > 0)
        {
            HandleAttack(enemies);
            // Update tower layer and sorting order based on all enemies
            UpdateLayerAndOrder();
        }
        else
        {
            ResetTowerLayer();
        }
    }

    protected abstract void HandleAttack(Collider2D[] enemies);

    private void UpdateLayerAndOrder()
    {
        if (spriteRenderer == null) return;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Enemies"));
        if (enemies.Length == 0)
        {
            ResetTowerLayer();
            return;
        }

        int aboveCount = 0;
        int belowCount = 0;
        float towerY = transform.position.y;

        foreach (Collider2D enemy in enemies)
        {
            float enemyY = enemy.transform.position.y;
            if (enemyY < towerY - LAYER_SWITCH_BUFFER) belowCount++;
            else if (enemyY > towerY + LAYER_SWITCH_BUFFER) aboveCount++;
        }

        // Determine layer based on majority of enemy positions
        if (aboveCount > belowCount)
        {
            spriteRenderer.sortingLayerName = aboveLayer;
        }
        else
        {
            spriteRenderer.sortingLayerName = baseLayer;
        }

        // Set sorting order based on inverted Y-position, adjusted by layer
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y) + (spriteRenderer.sortingLayerName == aboveLayer ? 10 : 0);
    }

    private void ResetTowerLayer()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = baseLayer;
            spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y);
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