using UnityEngine;

public class MiningTower : MonoBehaviour
{
    [SerializeField] private Healthbar healthbar;
    [SerializeField] private Animator animator;
    private float lifespan = 15f;
    private float currentHealth;
    private float deathTimer;
    private int maxHealth;
    private bool isDead;

    public void Initialize(int maxHealth)
    {
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;
        deathTimer = lifespan;
        isDead = false;

        animator.enabled = true;
        animator.Play("Mine Tower Animation", 0, 0f);

        healthbar.SetInitialHealth(maxHealth);
        healthbar.UpdateHealth(currentHealth);

        gameObject.tag = "MiningMachine";

        InvokeRepeating("DecreaseHealth", 0f, 0.1f);
    }

    private void DecreaseHealth()
    {
        if (isDead) return;

        if (deathTimer > 0)
        {
            deathTimer -= 0.1f;
            currentHealth = (deathTimer / lifespan) * maxHealth;

            healthbar.UpdateHealth(currentHealth);

            if (currentHealth <= 0 || deathTimer <= 0)
            {
                Die();
            }
        }
    }

    private void Die()
    {
        isDead = true;
        CancelInvoke("DecreaseHealth");

        healthbar.UpdateHealth(0);
        animator.enabled = false;

        gameObject.tag = "Untagged";
    }

    private void OnMouseDown()
    {
        if (isDead)
        {
            Revive();
        }
    }

    private void Revive()
    {
        isDead = false;
        currentHealth = maxHealth;
        deathTimer = lifespan;

        healthbar.UpdateHealth(currentHealth);

        animator.enabled = true;
        animator.Play("Mine Tower Animation", 0, 0f);

        gameObject.tag = "MiningMachine";

        InvokeRepeating("DecreaseHealth", 0f, 0.1f);
    }
}
