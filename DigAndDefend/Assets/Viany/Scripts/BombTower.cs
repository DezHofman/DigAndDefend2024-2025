using UnityEngine;

public class BombTower : Tower
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void HandleAttack(Collider2D[] enemies)
    {
        foreach (Collider2D enemyCollider in enemies)
        {
            BaseEnemy enemy = enemyCollider.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }
}