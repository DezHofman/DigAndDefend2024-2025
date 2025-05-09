using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TestScript : MonoBehaviour
{

    public Animator animator;
  
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))        
        {
            animator.SetBool("WD", true);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            animator.SetBool("WD", true);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            animator.SetBool("WD", false);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            animator.SetBool("WD", false);
        }
    }
}
