using UnityEngine;

public class ArcherTower : Tower
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void HandleAttack(Collider2D[] enemies)
    {
        if (enemies.Length > 0)
        {
            Collider2D nearestEnemy = enemies[0];
            float minDistance = Vector2.Distance(transform.position, nearestEnemy.transform.position);

            foreach (Collider2D enemyCollider in enemies)
            {
                float distance = Vector2.Distance(transform.position, enemyCollider.transform.position);
                if (distance < minDistance)
                {
                    nearestEnemy = enemyCollider;
                    minDistance = distance;
                }
            }

            BaseEnemy enemy = nearestEnemy.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }
}