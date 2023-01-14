using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/Gun")]
public class GunData : ScriptableObject
{
    [Header("Meta Data")]
    public new string name;
    [HideInInspector]
    public Collider playerCollider;
    [Tooltip("in shots per second")]
    public float fireRate;
    public bool semiAuto;


    [Header("Projectiles")]
    public float damage;
    public float bulletsPerShot;
    public float bulletSpeed;
    public float maxDistance;

    [Header("Reload")]
    public int clipSize;
    public float reloadTime;

    private void OnEnable()
    {
        // used for bullets to ignore player collider
        playerCollider = GameObject.Find("Player").GetComponent<Collider>();
    }
}
