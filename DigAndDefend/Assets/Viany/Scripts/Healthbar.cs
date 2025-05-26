using UnityEngine;

public class Healthbar : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float initialHealth = 100f;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float damagePerPress = 10f;

    [Header("Position Settings")]
    [SerializeField] private float worldOffset = 1.0f;
    [SerializeField] private float screenOffset = 5f;

    [Header("Color Settings")]
    [SerializeField] private Color healthColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color damageColor = new Color(0.8f, 0.2f, 0.2f);
    [SerializeField] private Color outlineColor = new Color(0, 0, 0);

    [Header("Size Settings")]
    [SerializeField] private float healthbarWidth = 100f;
    [SerializeField] private float healthbarHeight = 10f;

    private float health;
    private bool showHealthbar = false;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        health = initialHealth;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && health > 0)
        {
            health = Mathf.Max(0, health - damagePerPress);
            showHealthbar = true;
        }

        if (health <= 0)
        {
            showHealthbar = false;
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

        Vector3 worldPosition = transform.position + Vector3.up * worldOffset;
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