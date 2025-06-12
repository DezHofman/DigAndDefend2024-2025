using UnityEngine;

public class MouseFollowSprite : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private ShopManager shopManager;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("MouseFollowSprite requires a SpriteRenderer!");
        }
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
        }
        shopManager = FindFirstObjectByType<ShopManager>(); // Updated to FindFirstObjectByType
        if (shopManager == null)
        {
            Debug.LogError("ShopManager not found in scene!");
        }
    }

    private void Start()
    {
        // Hide initially (will show only in mine)
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    private void OnEnable()
    {
        // Reset state when enabled (e.g., after scene reload)
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    private void Update()
    {
        if (shopManager == null || mainCamera == null || spriteRenderer == null) return;

        // Use ShopManager's isInCaveArea to determine visibility
        bool isInMine = shopManager.IsInCaveArea();

        // Toggle visibility based on mine status
        spriteRenderer.enabled = isInMine;

        // Follow mouse only if visible
        if (isInMine)
        {
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = transform.position.z; // Keep original Z for layering
            transform.position = mousePosition;
        }
    }
}