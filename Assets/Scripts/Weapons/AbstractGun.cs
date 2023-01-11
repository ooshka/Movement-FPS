using System.Collections;
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
        InputManager.shootAction += Shoot;
        InputManager.reloadAction += Reload;

        currentAmmo = data.clipSize;
    }

    private void Shoot()
    {
        if (!reloading && currentAmmo > 0)
        {
            Debug.Log("Shoosting");
            currentAmmo--;
        }
    }

    private void Reload()
    {
        if (!reloading)
        {
            StartCoroutine(DoReload());
        }
    }

    private IEnumerator DoReload()
    {
        reloading = true;
        Debug.Log("Reloading");

        yield return new WaitForSeconds(data.reloadTime);

        currentAmmo = data.clipSize;
        reloading = false;
        Debug.Log("Finished Reloading");
    }
}
