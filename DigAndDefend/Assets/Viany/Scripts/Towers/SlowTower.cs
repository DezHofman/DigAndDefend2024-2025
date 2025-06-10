using UnityEngine;

public class SlowTower : Tower
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
                GameObject iceCream = Instantiate(projectilePrefab, transform.position + Vector3.up, Quaternion.identity);
                IceCreamProjectile iceCreamProj = iceCream.GetComponent<IceCreamProjectile>();
                if (iceCreamProj != null)
                {
                    iceCreamProj.Initialize(this, closestEnemy.transform);
                    Debug.Log($"SlowTower: Ice cream fired toward {closestEnemy.transform.position}");
                }
            }
        }
    }
}