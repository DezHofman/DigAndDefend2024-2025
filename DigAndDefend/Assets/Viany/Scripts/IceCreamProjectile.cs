using UnityEngine;

public class IceCreamProjectile : Projectile
{
    public float slowFactor = 0.5f; // 50% slow
    public float slowDuration = 2f; // 2 seconds

    protected override void OnHitEnemy(BaseEnemy enemy)
    {
        enemy.ApplySlow(slowFactor, slowDuration);
        Debug.Log($"IceCreamProjectile: Applied slow to enemy {enemy.name}: {slowFactor} for {slowDuration}s");
    }
}