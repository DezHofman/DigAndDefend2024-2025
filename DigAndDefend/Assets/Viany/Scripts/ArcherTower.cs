using UnityEngine;

public class ArcherTower : Tower
{
    [SerializeField] private float defaultShootDistance = 5f; // Distance to shoot if no enemy

    protected override void HandleAttack(Collider2D[] enemies)
    {
        Transform target = null;
        Vector3 targetPosition = transform.position + Vector3.right * defaultShootDistance; // Default to right

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
                target = closestEnemy.transform;
                targetPosition = closestEnemy.transform.position;
            }
        }

        SpawnProjectile(target, targetPosition);
    }
}