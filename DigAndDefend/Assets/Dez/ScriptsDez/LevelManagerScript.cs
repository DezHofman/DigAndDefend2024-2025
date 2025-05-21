using UnityEngine;

public class LevelManagerScript : MonoBehaviour
{
    public static LevelManagerScript main;

    public Transform[] Path;
    public Transform startPoint;
    private void Awake()
    {
        main = this;
    }
}
