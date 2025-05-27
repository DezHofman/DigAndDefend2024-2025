using UnityEngine;
using System.Collections;

public abstract class Tower : MonoBehaviour
{
    public float attackRange;
    public float attackSpeed;
    public float attackDamage;
    public float rangeDisplayDuration = 2f;
    public GameObject rangeIndicatorPrefab;
    protected float timeSinceLastAttack = 0f;
    protected CircleCollider2D attackCollider;
    private GameObject rangeIndicator;
    private static Tower activeTower;
    private float rangeVisibleTime = 0f;
    private bool isRangeVisible = false;

    protected virtual void Start()
    {
        attackCollider = GetComponent<CircleCollider2D>();
        if (attackCollider != null)
        {
            attackCollider.radius = attackRange;
            attackCollider.isTrigger = true;
        }
        gameObject.tag = "Tower";

        rangeIndicator = Instantiate(rangeIndicatorPrefab, transform.position, Quaternion.identity, transform);
        rangeIndicator.SetActive(false);
        rangeIndicator.layer = LayerMask.NameToLayer("Ignore Raycast");

        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart()
    {
        yield return null;
        attackCollider.radius = attackRange;
        rangeIndicator.transform.localScale = new Vector3(attackRange * 2, attackRange * 2, 1);
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