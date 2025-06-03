using UnityEngine;
using System.Collections;

public abstract class Tower : MonoBehaviour
{
    public float attackRange;
    public float attackSpeed;
    public float attackDamage;
    public float rangeDisplayDuration;
    public GameObject rangeIndicatorPrefab;
    protected float timeSinceLastAttack;
    protected CircleCollider2D attackCollider;
    private GameObject rangeIndicator;
    private static Tower activeTower;
    private float rangeVisibleTime;
    private bool isRangeVisible = false;
    private SpriteRenderer spriteRenderer;
    private Vector3 initialPosition;

    protected virtual void Start()
    {
        initialPosition = transform.position;
        Debug.Log($"{gameObject.name} - Initial position set: {initialPosition}");

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on " + gameObject.name + " or its children.");
            return;
        }

        attackCollider = GetComponent<CircleCollider2D>();
        if (attackCollider != null)
        {
            attackCollider.radius = attackRange;
            attackCollider.isTrigger = true;
        }
        gameObject.tag = "Tower";

        Debug.Log($"{gameObject.name} - Tower position after setup: {transform.position}, " +
                  $"Sprite local position: {spriteRenderer.transform.localPosition}, " +
                  $"Sprite world position: {spriteRenderer.transform.position}");

        rangeIndicator = Instantiate(rangeIndicatorPrefab, transform.position, Quaternion.identity, transform);
        rangeIndicator.transform.localScale = new Vector3(attackRange * 2, attackRange * 2, 1);
        rangeIndicator.SetActive(false);
        rangeIndicator.layer = LayerMask.NameToLayer("Ignore Raycast");

        if (transform.position != initialPosition)
        {
            Debug.LogWarning($"{gameObject.name} - Position reset detected, restoring to: {initialPosition}");
            transform.position = initialPosition;
        }

        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart()
    {
        yield return null;
        attackCollider.radius = attackRange;
        rangeIndicator.transform.localScale = new Vector3(attackRange * 2, attackRange * 2, 1);
        Debug.Log($"{gameObject.name} - Tower position in LateStart: {transform.position}");

        if (transform.position != initialPosition)
        {
            Debug.LogWarning($"{gameObject.name} - Position reset detected in LateStart, restoring to: {initialPosition}");
            transform.position = initialPosition;
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
            rangeVisibleTime += Time.deltaTime;
            if (rangeVisibleTime >= rangeDisplayDuration)
            {
                rangeIndicator.SetActive(false);
                isRangeVisible = false;
                if (activeTower == this)
                {
                    activeTower = null;
                }
            }
        }

        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"{gameObject.name} - Tower position in Update: {transform.position}");
            if (transform.position != initialPosition)
            {
                Debug.LogWarning($"{gameObject.name} - Position reset detected in Update, restoring to: {initialPosition}");
                transform.position = initialPosition;
            }
        }
    }

    void OnMouseDown()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Vector2.Distance(mousePos, transform.position) > 0.5f) return;

        if (activeTower != null && activeTower != this)
        {
            activeTower.rangeIndicator.SetActive(false);
            activeTower.isRangeVisible = false;
            activeTower.rangeVisibleTime = 0f;
        }
        activeTower = this;
        if (rangeIndicator != null)
        {
            bool wasVisible = rangeIndicator.activeSelf;
            rangeIndicator.SetActive(!wasVisible);
            isRangeVisible = !wasVisible;
            rangeVisibleTime = 0f;
        }
    }

    protected virtual void Attack()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, attackRange, LayerMask.GetMask("Enemies"));
        if (enemies.Length > 0)
        {
            HandleAttack(enemies);
        }
    }

    protected abstract void HandleAttack(Collider2D[] enemies);

    private void OnDestroy()
    {
        if (rangeIndicator != null)
        {
            Destroy(rangeIndicator);
        }
        if (activeTower == this)
        {
            activeTower = null;
        }
    }
}