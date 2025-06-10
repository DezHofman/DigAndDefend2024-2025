using UnityEngine;

public class MiningTower : MonoBehaviour
{
    [SerializeField] private Healthbar healthbar; // Reference to the Healthbar component
    private Animator animator; // Reference to the Animator component
    private float lifespan = 15f; // Lifespan in seconds
    private float currentHealth;
    private float deathTimer;
    private int maxHealth; // Store max health for revival
    private bool isDead = false; // Track if the tower is "dead"

    private void Awake()
    {
        // Ensure healthbar and animator are assigned
        if (healthbar == null)
        {
            healthbar = GetComponentInChildren<Healthbar>();
            if (healthbar == null)
            {
                Debug.LogError($"No Healthbar component found on {gameObject.name} or its children!");
            }
        }

        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError($"No Animator component found on {gameObject.name} or its children!");
        }

        // Ensure a Collider2D exists for click detection
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>().isTrigger = true; // Add a trigger collider for clicks
        }
    }

    public void Initialize(int maxHealth)
    {
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;
        deathTimer = lifespan;
        isDead = false;

        // Ensure animation is enabled
        if (animator != null)
        {
            animator.enabled = true;
            animator.Play("MiningAnimation", -1, 0f); // Reset to start of animation (replace "MiningAnimation" with your animation state name)
        }

        // Initialize healthbar
        if (healthbar != null)
        {
            healthbar.SetInitialHealth(maxHealth);
            healthbar.UpdateHealth(currentHealth);
            Debug.Log($"MiningTower initialized with maxHealth: {maxHealth}, currentHealth: {currentHealth}");
        }

        // Ensure the tag is set for resource generation
        gameObject.tag = "MiningMachine";

        // Start gradual health decrease
        InvokeRepeating("DecreaseHealth", 0f, 0.1f); // Check every 0.1 seconds
        Debug.Log($"MiningTower lifespan timer started for {lifespan} seconds");
    }

    private void DecreaseHealth()
    {
        if (isDead) return; // Skip if already dead

        if (deathTimer > 0)
        {
            deathTimer -= 0.1f;
            currentHealth = (deathTimer / lifespan) * maxHealth; // Linear decrease
            Debug.Log($"Decreasing health: {currentHealth}, timer: {deathTimer}");

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
        isDead = true;
        CancelInvoke("DecreaseHealth"); // Stop the timer

        if (healthbar != null)
        {
            healthbar.UpdateHealth(0); // Show death on healthbar
        }

        if (animator != null)
        {
            animator.enabled = false; // Stop (freeze) the animation at the current frame
            Debug.Log($"MiningTower {gameObject.name} animation stopped at {Time.time} seconds");
        }

        // Remove the tag to stop resource generation
        gameObject.tag = "Untagged";
        Debug.Log($"MiningTower {gameObject.name} is now static (dead) at {Time.time} seconds");
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

        // Reset healthbar
        if (healthbar != null)
        {
            healthbar.UpdateHealth(currentHealth);
        }

        // Resume animation
        if (animator != null)
        {
            animator.enabled = true; // Resume the animation
            animator.Play("MiningAnimation", -1, 0f); // Reset to start (replace "MiningAnimation" with your animation state name)
            Debug.Log($"MiningTower {gameObject.name} animation resumed");
        }

        // Restore tag for resource generation
        gameObject.tag = "MiningMachine";

        // Restart the lifespan timer
        InvokeRepeating("DecreaseHealth", 0f, 0.1f);
        Debug.Log($"MiningTower {gameObject.name} revived with full health: {currentHealth}");
    }
}