using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/Gun")]
public class GunData : ScriptableObject
{
    [Header("Meta Data")]
    public new string name;
    [Tooltip("in shots per second")]
    public float fireRate;
    public bool semiAuto;


    [Header("Projectiles")]
    public float damage;
    public float bulletsPerShot;
    public float bulletSpeed;

    [Header("Reload")]
    public int clipSize;
    public float reloadTime;

}
