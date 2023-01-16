using System.Collections;
using UnityEngine;

public abstract class AbstractGun : MonoBehaviour
{
    private Camera cam;
    private PlayerLook playerLook;
    public Transform muzzle;
    public GunData data;
    public AbstractBullet bullet;

    [SerializeField]
    private float currentAmmo;

    private bool reloading = false;
    private bool isWithinFireRate = true;
    private bool isShooting;
    private bool shouldResetRecoil;
    private int horizontalRecoilDirection = 0;

    private readonly Timer recoilTimer = new Timer(0, true);
    private Timer settlingPositionTimer;
    private float settlingAngle;
    private float lastVerticalAngle;

    // Start is called before the first frame update
    private void Start()
    {
        cam = GameObject.Find("Main_Camera").GetComponent<Camera>();
        GameObject player = GameObject.Find("Player");
        playerLook = player.GetComponent<PlayerLook>();

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
        settlingPositionTimer = new Timer(data.settlingPositionCooldown, false);
    }

    private void Update()
    {
        // need to check and see if we're shooting
        if (!data.semiAuto && isShooting)
        {
            Shoot();
        }

        RecoilUpdate();

        recoilTimer.Iterate(Time.deltaTime);
        settlingPositionTimer.Iterate(Time.deltaTime);
        // we store this to check if we aim upwards in PlayerLook
        // if we do then reset our settling position so we don't get outrageously large crosshair recoveries
        lastVerticalAngle = cam.transform.forward.y;
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

            if (data.verticalRecoilVelocity > 0)
            {
                SetRecoil();

            }
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

    private void SetRecoil()
    {
        // add some time to our vertical recoil timer so we can kick a bit
        recoilTimer.AddTime(1 / data.fireRate);
        // we should recover from this recoil
        shouldResetRecoil = true;

        if (data.horizontalRecoilVelocity > 0)
        {
            // horizontal recoil direction is stored as a signed counter
            if (Random.value > 0.5f)
            {
                horizontalRecoilDirection++;
            }
            else
            {
                horizontalRecoilDirection--;
            }
        }

        if (settlingPositionTimer.CanTriggerEvent() || cam.transform.forward.y > lastVerticalAngle)
        {
            // if there has been enough time between shots reset our crosshair recovery angle
            // OR if we're aiming higher than last frame (i.e. player has aimed upwards)
            settlingAngle = cam.transform.forward.y;
        }

        // reset our cooldown position timer
        settlingPositionTimer.Reset();
    }

    private void RecoilUpdate()
    {
        if (recoilTimer.CanTriggerEvent())
        {
            // we have time left so we are still recoiling
            playerLook.VerticalLook(data.verticalRecoilVelocity * Time.deltaTime);

            // horizontal recoil
            if (data.horizontalRecoilVelocity > 0)
            {
                int recoilDir = horizontalRecoilDirection == 0 ? horizontalRecoilDirection : horizontalRecoilDirection / Mathf.Abs(horizontalRecoilDirection);
                playerLook.HorizontalLook(recoilDir * data.horizontalRecoilVelocity * Time.deltaTime);
            }
        } else
        {
            // reset our horizontal direction
            horizontalRecoilDirection = 0;

            // no recoil time so reset out crosshair position if needed
            if (shouldResetRecoil && cam.transform.forward.y > settlingAngle)
            {
                // if we're aiming above our reset position and we need to reset
                playerLook.VerticalLook(-data.settlingVelocity * Time.deltaTime);
            } else
            {
                // if we've already recovered to our settling position set flag
                shouldResetRecoil = false;
            }
        }
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
