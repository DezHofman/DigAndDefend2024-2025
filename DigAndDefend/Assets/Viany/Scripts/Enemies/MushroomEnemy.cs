using UnityEngine;

public class MushroomEnemy : BaseEnemy
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float explosionDamage = 20f;

    protected override string GetEnemyType()
    {
        return "Mushroom";
    }

    protected override void OnDeath()
    {
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 1.0f);
        }

        Collider2D[] hitBarricades = Physics2D.OverlapCircleAll(transform.position, explosionRadius, LayerMask.GetMask("Barricades"));
        foreach (Collider2D barricade in hitBarricades)
        {
            Barricade barricadeComponent = barricade.GetComponent<Barricade>();
            if (barricadeComponent != null)
            {
                barricadeComponent.TakeDamage(explosionDamage);
            }
        }
        Destroy(gameObject);
    }
}