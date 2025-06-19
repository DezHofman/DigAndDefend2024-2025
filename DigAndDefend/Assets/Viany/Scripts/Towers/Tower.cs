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

        rangeIndicator = Instantiate(rangeIndicatorPrefab, transform.position, Quaternion.identity, transform);
        rangeIndicator.transform.localScale = new Vector3(attackRange * 2, attackRange * 2, 1);
        rangeIndicator.SetActive(false);
        rangeIndicator.layer = LayerMask.NameToLayer("Ignore Raycast");
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

        if (Input.GetMouseButtonDown(0) && !IsCanvasOpen())
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
        Collider2D hitEnemy = Physics2D.OverlapCircle(transform.position, attackRange, LayerMask.GetMask("Enemies"));
        if (hitEnemy != null)
        {
            HandleAttack(new Collider2D[] { hitEnemy });
        }
    }

    protected abstract void HandleAttack(Collider2D[] enemies);

    private bool IsCanvasOpen()
    {
        GameManager gm = FindFirstObjectByType<GameManager>();
        return gm != null && gm.IsAnyCanvasOpen();
    }

    private void OnDestroy()
    {
        if (rangeIndicator != null)
        {
            Destroy(rangeIndicator);
        }
    }
}
