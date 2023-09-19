using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/BulletData")]
public class BulletData : ScriptableObject
{
    [Tooltip("We need to know what is making this bullet so we can ignore its collider")]
    public enum Parent
    {
        PLAYER, ENEMY
    }

    public Parent parent;
    public float baseDamage;
    public float bulletSpeed;
    public float maxDistance;
}
