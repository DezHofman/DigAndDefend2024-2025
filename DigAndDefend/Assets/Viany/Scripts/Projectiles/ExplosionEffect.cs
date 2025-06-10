using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public float duration = 1f;

    void Start()
    {
        Destroy(gameObject, duration);
    }
}