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

    private bool reloading;

    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.Find("Main_Camera").GetComponent<Camera>();

        // initialize Action listeners
        InputManager.shootAction += Shoot;
        InputManager.reloadAction += Reload;

        currentAmmo = data.clipSize;
    }

    private void Shoot()
    {
        if (!reloading && currentAmmo > 0)
        {
            AbstractBullet bullet = Instantiate(this.bullet, muzzle.position, Quaternion.identity);

            bullet.transform.forward = CalcShotDirection();

            bullet.GetComponent<Rigidbody>().AddForce(data.bulletSpeed * bullet.transform.forward, ForceMode.VelocityChange);

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

    private Vector3 CalcShotDirection()
    {
        float calibrationDistance = 500;
        Vector3 cameraAimPoint = cam.transform.forward * calibrationDistance + cam.transform.position;
        Vector3 bulletDirection = (cameraAimPoint - muzzle.position).normalized;
        return bulletDirection;
    }
}
