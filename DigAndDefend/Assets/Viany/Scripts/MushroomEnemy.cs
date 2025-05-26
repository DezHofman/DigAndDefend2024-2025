using UnityEngine;

public class MushroomEnemy : BaseEnemy
{
    public float buffRange = 2f;
    public float speedBuff = 0.2f;

    protected override void OnDeath()
    {
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, buffRange, LayerMask.GetMask("Enemies"));
        foreach (Collider2D enemyCollider in nearbyEnemies)
        {
            BaseEnemy enemy = enemyCollider.GetComponent<BaseEnemy>();
            if (enemy != null && enemy != this)
            {
                enemy.speed += speedBuff;
            }
        }
        base.OnDeath();
    }

    public override void TakeDamage(float amount)
    {
        health -= (int)amount;
        if (health <= 0)
        {
            OnDeath();
        }
        else
        {
            Collider2D barricade = Physics2D.OverlapCircle(transform.position, 0.5f, LayerMask.GetMask("Barricades"));
            if (barricade != null)
            {
                Destroy(barricade.gameObject);
            }
        }
    }
}