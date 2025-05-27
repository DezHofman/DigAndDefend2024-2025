using UnityEngine;

public class GolemEnemy : BaseEnemy
{
    private void Start()
    {
        canFly = false;
        health = 1000;
        speed = 0.75f;
        damageToVillage = 15;
    }
}