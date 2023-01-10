using UnityEngine;

public abstract class AbstractGun : MonoBehaviour
{

    public Transform muzzle;
    public GunData data;

    [SerializeField]
    private float currentAmmo;

    private bool reloading;

    // Start is called before the first frame update
    void Start()
    {
        // initialize Action listeners
        InputManager.shoot += Shoot;

        currentAmmo = data.clipSize;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Shoot()
    {
        Debug.Log("Shoosting");
        currentAmmo--;
    }
}
