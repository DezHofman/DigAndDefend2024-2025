using UnityEngine;

public class FirePoisonTower : Tower
{
    public float dotDamagePerSecond = 5f;
    public float dotDuration = 3f;

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
                enemy.ApplyDoT(dotDamagePerSecond, dotDuration);
            }
        }
    }
}