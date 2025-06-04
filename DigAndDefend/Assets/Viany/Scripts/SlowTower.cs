using UnityEngine;

public class SlowTower : Tower
{
    protected override void HandleAttack(Collider2D[] enemies)
    {
        Collider2D closestEnemy = null;
        float closestDistance = float.MaxValue;
        foreach (Collider2D enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        if (closestEnemy != null)
        {
            SpawnProjectile(closestEnemy.transform, closestEnemy.transform.position);
        }
    }
}