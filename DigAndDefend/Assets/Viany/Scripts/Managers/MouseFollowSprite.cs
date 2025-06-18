using UnityEngine;

public class MouseFollowSprite : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ShopManager shopManager;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        spriteRenderer.enabled = false;
    }

    private void Update()
    {
        bool isInMine = shopManager.IsInCaveArea();

        spriteRenderer.enabled = isInMine;

        if (isInMine)
        {
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = transform.position.z;
            transform.position = mousePosition;
        }
    }
}