using UnityEngine;

public class Barricade : MonoBehaviour
{
    public float maxHealth;
    private float health;
    private Healthbar healthbar;

    private void Start()
    {
        health = maxHealth;
        healthbar = GetComponent<Healthbar>();
        if (healthbar != null)
        {
            healthbar.SetInitialHealth(maxHealth);
        }
        gameObject.tag = "Barricade";
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (healthbar != null)
        {
            healthbar.UpdateHealth(health);
        }
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}