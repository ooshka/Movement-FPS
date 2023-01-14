using System.Collections;
using UnityEngine;

public abstract class AbstractGun : MonoBehaviour
{
    private Camera cam;
    public Transform muzzle;
    public GunData data;
    public AbstractBullet bullet;

    [SerializeField]
    private float currentAmmo;

    private bool reloading = false;
    private bool isWithinFireRate = true;
    private bool isShooting;

    // Start is called before the first frame update
    private void Start()
    {
        cam = GameObject.Find("Main_Camera").GetComponent<Camera>();

        // initialize Action listeners
        if (data.semiAuto)
        {
            InputManager.shootStartAction += Shoot;
        } else
        {
            InputManager.shootStartAction += SetShooting;
            InputManager.shootEndAction += SetNotShooting;
        }

        InputManager.reloadAction += Reload;

        currentAmmo = data.clipSize;
    }

    private void Update()
    {
        // need to check and see if we're shooting
        if (!data.semiAuto && isShooting)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (!reloading && currentAmmo > 0 && isWithinFireRate)
        {
            // TODO: we may want to recycle bullets rather than instantiate
            AbstractBullet bullet = Instantiate(this.bullet, muzzle.position, Quaternion.identity);
            bullet.transform.forward = CalcShotDirection();

            bullet.GetComponent<Rigidbody>().AddForce(data.bulletSpeed * bullet.transform.forward, ForceMode.VelocityChange);

            if (data.bulletsPerShot > 1)
            {
                // TODO: handle multiple bullet guns like shotguns
            }

            currentAmmo--;

            StartCoroutine(HandleFireRate());
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

    private IEnumerator HandleFireRate()
    {
        isWithinFireRate = false;

        float fireFrequency = 1f / data.fireRate;

        yield return new WaitForSeconds(fireFrequency);

        isWithinFireRate = true;
    }

    private Vector3 CalcShotDirection()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit, data.maxDistance)) {
            targetPoint = hit.point;
        } else
        {
            // if we didn't hit anything aim at max distance point
            targetPoint = ray.GetPoint(data.maxDistance);
        }

        Vector3 bulletDirection = (targetPoint - muzzle.position).normalized;
        return bulletDirection;
    }

    private void SetShooting()
    {
        isShooting = true;
    }

    private void SetNotShooting()
    {
        isShooting = false;
    }
}
