using UnityEngine;

public class MiningTower : MonoBehaviour
{
    [SerializeField] private Healthbar healthbar;
    private float lifespan = 15f;
    private float currentHealth;
    private float deathTimer;

    private void Awake()
    {
        if (healthbar == null)
        {
            healthbar = GetComponentInChildren<Healthbar>();
            if (healthbar == null)
            {
                Debug.LogError($"No Healthbar component found on {gameObject.name} or its children!");
            }
        }
    }

    public void Initialize(int maxHealth)
    {
        currentHealth = maxHealth;
        deathTimer = lifespan;

        if (healthbar != null)
        {
            healthbar.SetInitialHealth(maxHealth);
            healthbar.UpdateHealth(currentHealth);
        }

        InvokeRepeating("DecreaseHealth", 0f, 0.1f);
    }

    private void DecreaseHealth()
    {
        if (deathTimer > 0)
        {
            deathTimer -= 0.1f;
            currentHealth = (deathTimer / lifespan) * healthbar.maxHealth;

            if (healthbar != null)
            {
                healthbar.UpdateHealth(currentHealth);
            }

            if (currentHealth <= 0 || deathTimer <= 0)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        if (healthbar != null)
        {
            healthbar.UpdateHealth(0);
        }
        Destroy(gameObject);
    }
}