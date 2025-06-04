using UnityEngine;

public class Healthbar : MonoBehaviour
{
    [SerializeField] private float initialHealth;
    [SerializeField] private float maxHealth;
    [SerializeField] private float worldOffset;
    [SerializeField] private float screenOffset;
    [SerializeField] private Color healthColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color damageColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color outlineColor = new Color(0, 0, 0);
    [SerializeField] private float healthbarWidth;
    [SerializeField] private float healthbarHeight;
    private float health;
    private bool showHealthbar = false;
    private Camera mainCamera;
    private BoxCollider2D boxCollider;

    private void Start()
    {
        mainCamera = Camera.main;
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            Debug.LogError($"No BoxCollider2D found on {gameObject.name}. Healthbar may be misaligned.");
        }
        health = initialHealth;
    }

    private void Update()
    {
    }

    public void SetInitialHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        health = newMaxHealth;
    }

    public void UpdateHealth(float newHealth)
    {
        Debug.Log("Updating health to: " + newHealth + ", Max: " + maxHealth);
        health = Mathf.Clamp(newHealth, 0, maxHealth);
        if (newHealth < initialHealth)
        {
            showHealthbar = true;
        }
    }

    private void OnGUI()
    {
        if (!showHealthbar) return;

        Texture2D damageTexture = new Texture2D(1, 1);
        damageTexture.SetPixel(0, 0, damageColor);
        damageTexture.Apply();

        Texture2D healthTexture = new Texture2D(1, 1);
        healthTexture.SetPixel(0, 0, healthColor);
        healthTexture.Apply();

        Texture2D outlineTexture = new Texture2D(1, 1);
        outlineTexture.SetPixel(0, 0, outlineColor);
        outlineTexture.Apply();

        // Use BoxCollider2D bounds to position the healthbar
        Vector3 worldPosition;
        if (boxCollider != null)
        {
            // Position above the top of the collider
            Vector3 colliderTop = transform.position + (Vector3)boxCollider.offset + Vector3.up * (boxCollider.size.y / 2);
            worldPosition = colliderTop + Vector3.up * worldOffset;
        }
        else
        {
            // Fallback to original behavior if no collider
            worldPosition = transform.position + Vector3.up * worldOffset;
        }

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        float healthbarX = screenPosition.x - healthbarWidth / 2;
        float healthbarY = Screen.height - screenPosition.y - healthbarHeight - screenOffset;

        GUI.DrawTexture(new Rect(healthbarX - 4, healthbarY - 4, healthbarWidth + 8, healthbarHeight + 8), outlineTexture);
        GUI.DrawTexture(new Rect(healthbarX - 4, healthbarY - 4, healthbarWidth + 8, 4), outlineTexture);
        GUI.DrawTexture(new Rect(healthbarX - 4, healthbarY + healthbarHeight, healthbarWidth + 8, 4), outlineTexture);
        GUI.DrawTexture(new Rect(healthbarX - 4, healthbarY - 4, 4, healthbarHeight + 8), outlineTexture);
        GUI.DrawTexture(new Rect(healthbarX + healthbarWidth, healthbarY - 4, 4, healthbarHeight + 8), outlineTexture);

        GUI.DrawTexture(new Rect(healthbarX, healthbarY, healthbarWidth, healthbarHeight), damageTexture);

        float currentWidth = (health / maxHealth) * healthbarWidth;
        GUI.DrawTexture(new Rect(healthbarX, healthbarY, currentWidth, healthbarHeight), healthTexture);

        Destroy(damageTexture);
        Destroy(healthTexture);
        Destroy(outlineTexture);
    }
}