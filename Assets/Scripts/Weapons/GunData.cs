using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/GunData")]
public class GunData : ScriptableObject
{
    public enum GunType
    {
        PISTOL, SMG, SHOTGUN
    }

    public enum SLOT
    {
        PRIMARY, SECONDARY, POWER
    }

    [Header("Meta Data")]
    public new string name;
    public GunType type;
    public SLOT slot;
    [HideInInspector]
    public Collider playerCollider = null;
    [Tooltip("in shots per second")]
    public float fireRate;
    public bool semiAuto;
    [Tooltip("set in %")]
    public float adsFOVChange;
    [Tooltip("the amount of time to transition to new fov")]
    public float adsFOVChangeTime;

    [Header("Recoil")]
    public float verticalRecoilVelocity;
    [Tooltip("Not currently being used, but may have to be for semi auto guns")]
    public float verticalRecoilTime;
    public float horizontalRecoilVelocity;
    public float settlingVelocity;
    [Tooltip("this is the interval of time between shots needed to reset our crosshair's 'recovery' position")]
    public float settlingPositionCooldown;

    [Header("Projectiles")]
    public float bulletsPerShot;


    [Header("Reload")]
    public int clipSize;
    public float reloadTime;
}
