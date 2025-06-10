using UnityEngine;

public class EnemySpriteController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void UpdateSpriteDirection(Vector2 direction)
    {
        animator.SetBool("Right", false);
        animator.SetBool("Left", false);
        animator.SetBool("Up", false);
        animator.SetBool("Down", false);
        spriteRenderer.flipX = false;

        if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
        {
            if (direction.y > 0)
            {
                animator.SetBool("Up", true);
                spriteRenderer.flipX = false;
                Debug.Log("Moving Up");
            }
            else if (direction.y < 0)
            {
                animator.SetBool("Down", true);
                spriteRenderer.flipX = true;
                Debug.Log("Moving Down");
            }
        }
        else
        {
            if (direction.x > 0)
            {
                animator.SetBool("Right", true);
                spriteRenderer.flipX = true;
                Debug.Log("Moving Right");
            }
            else if (direction.x < 0)
            {
                animator.SetBool("Left", true);
                spriteRenderer.flipX = false;
                Debug.Log("Moving Left");
            }
        }
    }
}