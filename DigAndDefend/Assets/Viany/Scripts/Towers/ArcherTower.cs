using UnityEngine;

public class ArcherTower : Tower
{
    protected override void HandleAttack(Collider2D[] enemies)
    {
        if (enemies.Length > 0)
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
                GameObject arrow = Instantiate(projectilePrefab, transform.position + Vector3.up, Quaternion.identity);
                arrow.GetComponent<ArrowProjectile>().Initialize(this, closestEnemy.transform);
            }
        }
    }
}