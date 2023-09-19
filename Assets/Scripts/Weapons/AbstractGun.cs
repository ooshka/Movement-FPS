using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractGun : MonoBehaviour
{
    private Camera cam;
    private PlayerLook playerLook;
    private StateController stateController;
    private Crosshair crosshair;
    public Transform muzzle;
    [SerializeField]
    public GunData gunData;
    [SerializeField]
    private BulletData bulletData;

    public AbstractBullet bullet;

    private float currentAmmo;

    private bool reloading = false;
    private bool isWithinFireRate = true;
    private bool isShooting;
    private bool shouldResetRecoil;
    private bool isAds = false;
    private int horizontalRecoilDirection = 0;

    private readonly Timer recoilTimer = new Timer(0, true);
    private Timer settlingPositionTimer;
    private float settlingAngle;
    private float lastVerticalAngle;
    private float defaultFOV;

    private List<StateController.State> gunState = new List<StateController.State>();

    // Start is called before the first frame update
    private void Start()
    {
        cam = GameObject.Find("Main_Camera").GetComponent<Camera>();
        defaultFOV = cam.fieldOfView;
        GameObject player = GameObject.Find("Player");
        playerLook = player.GetComponent<PlayerLook>();
        stateController = player.GetComponent<StateController>();
        gunData.playerCollider = player.GetComponent<Collider>();

        crosshair = GameObject.Find("HUD_Canvas").GetComponent<Crosshair>();

        // initialize Action listeners
        if (gunData.semiAuto)
        {
            InputManager.shootStartAction += Shoot;
        } else
        {
            InputManager.shootStartAction += SetShooting;
            InputManager.shootEndAction += SetNotShooting;
        }

        InputManager.reloadAction += Reload;
        InputManager.adsStartAction += SetADS;
        InputManager.adsEndAction += SetNotADS;

        currentAmmo = gunData.clipSize;
        settlingPositionTimer = new Timer(gunData.settlingPositionCooldown, false);
    }

    private void Update()
    {
        // need to check and see if we're shooting
        if (!gunData.semiAuto && isShooting)
        {
            Shoot();
        }

        RecoilUpdate();

        recoilTimer.Iterate(Time.deltaTime);
        settlingPositionTimer.Iterate(Time.deltaTime);
        // we store this to check if we aim upwards in PlayerLook
        // if we do then reset our settling position so we don't get outrageously large crosshair recoveries
        lastVerticalAngle = cam.transform.forward.y;

        stateController.SetGunState(gunState);
        gunState.Remove(StateController.State.SHOOTING);
        gunState.Remove(StateController.State.RELOADING);
    }

    private void LateUpdate()
    {
        if (isAds)
        {
            cam.fieldOfView = MotionCurves.LinearInterp(cam.fieldOfView, defaultFOV, defaultFOV * (1 - gunData.adsFOVChange / 100), gunData.adsFOVChangeTime);
            if (!DebugFlags.IsAlwaysADS())
            {
                crosshair.HideCrosshair();
            }
        } else
        {
            cam.fieldOfView = MotionCurves.LinearInterp(cam.fieldOfView, defaultFOV * (1 - gunData.adsFOVChange / 100), defaultFOV, gunData.adsFOVChangeTime);
            crosshair.ShowCrosshair();
        }
    }

    private void Shoot()
    {
        if (!reloading && currentAmmo > 0 && isWithinFireRate)
        {
            gunState.Add(StateController.State.SHOOTING);
            // TODO: we may want to recycle bullets rather than instantiate
            AbstractBullet bullet = Instantiate(this.bullet, muzzle.position, Quaternion.identity);
            bullet.Fire(CalcShotDirection());

            if (gunData.bulletsPerShot > 1)
            {
                // TODO: handle multiple bullet guns like shotguns
            }

            currentAmmo--;

            if (gunData.verticalRecoilVelocity > 0)
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
        gunState.Add(StateController.State.RELOADING);
        reloading = true;

        yield return new WaitForSeconds(gunData.reloadTime);

        currentAmmo = gunData.clipSize;
        reloading = false;
    }

    private IEnumerator HandleFireRate()
    {
        isWithinFireRate = false;

        float timeBetweenShots = 1f / gunData.fireRate;

        yield return new WaitForSeconds(timeBetweenShots);

        isWithinFireRate = true;
    }

    private Vector3 CalcShotDirection()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit, bulletData.maxDistance)) {
            targetPoint = hit.point;
        } else
        {
            // if we didn't hit anything aim at max distance point
            targetPoint = ray.GetPoint(bulletData.maxDistance);
        }

        Vector3 bulletDirection = (targetPoint - muzzle.position).normalized;
        return bulletDirection;
    }

    private void SetRecoil()
    {
        // add some time to our vertical recoil timer so we can kick a bit
        recoilTimer.AddTime(1 / gunData.fireRate);
        // we should recover from this recoil
        shouldResetRecoil = true;

        if (gunData.horizontalRecoilVelocity > 0)
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
            playerLook.VerticalLook(gunData.verticalRecoilVelocity * Time.deltaTime);

            // horizontal recoil
            if (gunData.horizontalRecoilVelocity > 0)
            {
                int recoilDir = horizontalRecoilDirection == 0 ? horizontalRecoilDirection : horizontalRecoilDirection / Mathf.Abs(horizontalRecoilDirection);
                playerLook.HorizontalLook(recoilDir * gunData.horizontalRecoilVelocity * Time.deltaTime);
            }
        } else
        {
            // reset our horizontal direction
            horizontalRecoilDirection = 0;

            // no recoil time so reset out crosshair position if needed
            if (shouldResetRecoil && cam.transform.forward.y > settlingAngle)
            {
                // if we're aiming above our reset position and we need to reset
                playerLook.VerticalLook(-gunData.settlingVelocity * Time.deltaTime);
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

    private void SetADS()
    {
        gunState.Add(StateController.State.ADS);
        isAds = true;
    }

    private void SetNotADS()
    {
        gunState.Remove(StateController.State.ADS);
        isAds = false;
    }

    private void OnDestroy()
    {
        // initialize Action listeners
        if (gunData.semiAuto)
        {
            InputManager.shootStartAction -= Shoot;
        }
        else
        {
            InputManager.shootStartAction -= SetShooting;
            InputManager.shootEndAction -= SetNotShooting;
        }

        InputManager.reloadAction -= Reload;
        InputManager.adsStartAction -= SetADS;
        InputManager.adsEndAction -= SetNotADS;
    }
}
