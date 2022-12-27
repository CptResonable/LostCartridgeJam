using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {
    [SerializeField] private float timeBetweenShots;
    [SerializeField] private Transform tMuzzle;

    [SerializeField] private GameObject prefabSFX;
    [SerializeField] private GameObject prefabBullet;
    [SerializeField] private GameObject prefab_vfxMuzzleFlash;

    [SerializeField] private int magSize;
    [SerializeField] private float muzzleVelocity;
    [SerializeField] private float reloadTime;
    [SerializeField] private float damage;

    [SerializeField] private AnimationCurve recoilCurve;
    [SerializeField] private AnimationCurve horizontalRecoilCurve;
    [SerializeField] private AnimationCurve recoilResetSpeedCurve;
    [SerializeField] private float backRecoil;
    [SerializeField] private float upRecoil;
    [SerializeField] private float horizontalRecoil;
    [SerializeField] private float recoilIncreasPerBullet;
    [SerializeField] private float recoilResetRate;
    [SerializeField] private float recoilT;
    [SerializeField] private float headUpRecoil;
    [SerializeField] private float headHorizontalRecoil;
    [SerializeField] private float horizontalChangeSpeed;

    [SerializeField] private Player player;
    [SerializeField] public bool isAuto;

    public bool consumeAmmo;
    public int ammoReserve;

    public Transform tOffHandTarget;
    public Vector3 targetHandPosition;
    public Vector3 targetAdsHandPosition;

    private float cooldown = 0;
    private float timeSinceLastShot = 0;

    public int bulletsInMagCount = 30;
    private Coroutine reloadCorutine;
    public bool isReloading = false;

    public bool hasSlideStop = true;

    public delegate void GunFiredDelegate(Vector3 rotationalRecoil, Vector3 translationalRecoil);
    public event GunFiredDelegate gunFiredEvent;
    public event Delegates.FloatDelegate reloadStartedEvent;
    public event Delegates.EmptyDelegate reloadFinishedEvent;

    private void LateUpdate() {

        cooldown -= Time.deltaTime;

        recoilT -= Time.deltaTime * recoilResetSpeedCurve.Evaluate(timeSinceLastShot);
        timeSinceLastShot += Time.deltaTime;

        if (recoilT < 0)
            recoilT = 0;
    }

    public void TryFire() {
        if (cooldown <= 0 && bulletsInMagCount > 0 && !isReloading) {
            cooldown = timeBetweenShots;
            Fire();
        }
    }

    public void Reload() {
        if (consumeAmmo) {
            if (ammoReserve <= 0) {
                return;
            }
            else {
            }
        }

        if (!isReloading)
            reloadCorutine = StartCoroutine(ReloadCorutine());
    }

    public void ReloadCanceled() {
        if (isReloading) {
            isReloading = false;
            StopCoroutine(reloadCorutine);
        }
    }

    private void Fire() {
        timeSinceLastShot = 0;
        bulletsInMagCount--;

        Recoil();

        GameObject goBullet = EZ_Pooling.EZ_PoolManager.Spawn(prefabBullet.transform, tMuzzle.position - tMuzzle.forward * 0.05f, tMuzzle.rotation).gameObject;
        Bullet bullet = goBullet.GetComponent<Bullet>();
        bullet.Fire(tMuzzle.forward * muzzleVelocity, damage);

        // SFX
        GameObject goSFX = EZ_Pooling.EZ_PoolManager.Spawn(prefabSFX.transform, tMuzzle.position, tMuzzle.rotation).gameObject;
        SFX sfx = goSFX.GetComponent<SFX>();
        sfx.Play();

        // VFX
        GameObject goMuzzle = EZ_Pooling.EZ_PoolManager.Spawn(prefab_vfxMuzzleFlash.transform, tMuzzle.position, tMuzzle.rotation).gameObject;
        Vfx_muzzleFlash muzzleFlash = goMuzzle.GetComponent<Vfx_muzzleFlash>();
        muzzleFlash.Initiate(tMuzzle);
     
    }

    private void Recoil() {
        if (isAuto) {
            Rigidbody rb = transform.parent.GetComponent<Rigidbody>();

            float force = recoilCurve.Evaluate(recoilT);
            float horizontalForceScale = (Mathf.PerlinNoise(recoilT * horizontalChangeSpeed, 321.43f) - 0.2f) * 2 * horizontalRecoilCurve.Evaluate(recoilT);
            //rb.AddForce(-transform.forward * ((force * 5) + 5) , ForceMode.Impulse);
            //rb.AddForce(transform.right * horizontalForceScale * 5, ForceMode.Impulse);
            //rb.AddTorque(transform.right * -((force * 1.25f) + 0.75f), ForceMode.Impulse);
            //rb.AddTorque(transform.up * horizontalForceScale * 1.25f, ForceMode.Impulse);
            rb.AddForce(-transform.forward * ((force * 5) + 5), ForceMode.Impulse);
            rb.AddForce(transform.right * horizontalForceScale * 4, ForceMode.Impulse);
            rb.AddTorque(transform.right * -((force * 2f) + 0.0f), ForceMode.Impulse);
            rb.AddTorque(transform.up * horizontalForceScale * 1.25f, ForceMode.Impulse);
            //rb.AddTorque(transform.up * ((force * 1) + 0.25f) * horizontalForceScale, ForceMode.Impulse);

            gunFiredEvent?.Invoke(new Vector3((force * 3) + 2, horizontalForceScale * 5.5f), Vector3.zero);

            recoilT += recoilIncreasPerBullet;
        }
        else {
            Rigidbody rb = transform.parent.GetComponent<Rigidbody>();

            float force = recoilCurve.Evaluate(recoilT);
            float horizontalForceScale = (Mathf.PerlinNoise(recoilT * horizontalChangeSpeed, 321.43f) - 0.2f) * 2 * horizontalRecoilCurve.Evaluate(recoilT);
            rb.AddForce(-transform.forward * ((force * 5) + 4), ForceMode.Impulse);
            rb.AddForce(transform.right * (horizontalForceScale * 4 + 0.2f), ForceMode.Impulse);
            rb.AddTorque(transform.right * -((force * 1f) + 0.7f), ForceMode.Impulse);
            rb.AddTorque(transform.up * ((horizontalForceScale * 0.5f) + Random.Range(-0.1f, 0.1f)), ForceMode.Impulse);

            gunFiredEvent?.Invoke(new Vector3((force * 6) + 4, horizontalForceScale * 10.5f), Vector3.zero);

            recoilT += recoilIncreasPerBullet;
        }
    }

    private IEnumerator ReloadCorutine() {
        isReloading = true;
        reloadStartedEvent?.Invoke(reloadTime);
        float t = 0;

        while (t < 1) {
            t += Time.deltaTime / reloadTime;
            yield return new WaitForEndOfFrame();
        }

        isReloading = false;

        if (consumeAmmo && ammoReserve < magSize) {
            bulletsInMagCount = ammoReserve;
            ammoReserve = 0;
        }
        else {
            ammoReserve -= (magSize - bulletsInMagCount);
            bulletsInMagCount = magSize;
        }
    }
}
