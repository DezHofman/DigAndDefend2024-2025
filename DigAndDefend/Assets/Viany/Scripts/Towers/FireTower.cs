using UnityEngine;

public class FireTower : Tower
{
    public float dotDamagePerSecond = 5f;
    public float dotDuration = 3f;

    protected override void HandleAttack(Collider2D[] enemies)
    {
        foreach (Collider2D enemyCollider in enemies)
        {
            BaseEnemy enemy = enemyCollider.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                GameObject fireball = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                FireballProjectile projectile = fireball.GetComponent<FireballProjectile>();
                if (projectile != null)
                {
                    projectile.Initialize(this, enemy.transform);
                    projectile.dotDamagePerSecond = dotDamagePerSecond;
                    projectile.dotDuration = dotDuration;
                }
            }
        }
    }
}