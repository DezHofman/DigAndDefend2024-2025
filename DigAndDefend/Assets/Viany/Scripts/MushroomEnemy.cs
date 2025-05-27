using UnityEngine;

public class MushroomEnemy : BaseEnemy
{
    private void Start()
    {
        canFly = false;
        health = 100;
        speed = 2f;
        damageToVillage = 10;
    }
}