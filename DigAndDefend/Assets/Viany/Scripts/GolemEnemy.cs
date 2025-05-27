using UnityEngine;

public class GolemEnemy : BaseEnemy
{
    private void Start()
    {
        canFly = false;
        health = 200;
        speed = 2f;
        damageToVillage = 10;
    }
}