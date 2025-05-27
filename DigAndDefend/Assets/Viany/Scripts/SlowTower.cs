using UnityEngine;

public class SlowTower : Tower
{
    public float slowFactor = 0.3f;
    public float slowDuration = 3f;

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
                enemy.ApplySlow(slowFactor);
            }
        }
    }
}