using UnityEngine;

public class Barricade : MonoBehaviour
{
    public float maxHealth;
    private float health;
    [SerializeField] private Healthbar healthbar;

    private void Start()
    {
        health = maxHealth;
        healthbar.SetInitialHealth(maxHealth);
        gameObject.tag = "Barricade";
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        healthbar.UpdateHealth(health);
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}