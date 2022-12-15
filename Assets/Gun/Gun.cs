using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {
    [SerializeField] private float timeBetweenShots;
    [SerializeField] private Transform tMuzzle;

    [SerializeField] private GameObject prefabSFX;
    [SerializeField] private GameObject prefabBullet;
    [SerializeField] private GameObject prefab_vfxMuzzleFlash;

    [SerializeField] private float muzzleVelocity;

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

    private float cooldown = 0;
    private float timeSinceLastShot = 0;

    public int bulletsInMagCount = 10;

    public bool hasSlideStop = true;

    public delegate void GunFiredDelegate(Vector3 rotationalRecoil, Vector3 translationalRecoil);
    public event GunFiredDelegate gunFiredEvent;
    public event Delegates.EmptyDelegate reloadFinishedEvent;

    private void LateUpdate() {

        cooldown -= Time.deltaTime;

        recoilT -= Time.deltaTime * recoilResetSpeedCurve.Evaluate(timeSinceLastShot);
        timeSinceLastShot += Time.deltaTime;

        if (recoilT < 0)
            recoilT = 0;
    }

    public void TryFire() {
        if (cooldown <= 0) {
            cooldown = timeBetweenShots;
            Fire();
        }
    }


    private void Fire() {
        timeSinceLastShot = 0;

        Recoil();

        GameObject goBullet = EZ_Pooling.EZ_PoolManager.Spawn(prefabBullet.transform, tMuzzle.position, tMuzzle.rotation).gameObject;
        Bullet bullet = goBullet.GetComponent<Bullet>();
        bullet.Fire(tMuzzle.forward * muzzleVelocity);

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
        Rigidbody rb = transform.parent.GetComponent<Rigidbody>();

        float force = recoilCurve.Evaluate(recoilT);
        float horizontalForceScale = (Mathf.PerlinNoise(recoilT * horizontalChangeSpeed, 321.43f) - 0.2f) * 2 * horizontalRecoilCurve.Evaluate(recoilT);
        rb.AddForce(-transform.forward * ((force * 5) + 5) , ForceMode.Impulse);
        rb.AddForce(transform.right * horizontalForceScale * 5, ForceMode.Impulse);
        rb.AddTorque(transform.right * -((force * 1.25f) + 0.75f), ForceMode.Impulse);
        rb.AddTorque(transform.up * horizontalForceScale * 1.25f, ForceMode.Impulse);
        //rb.AddTorque(transform.up * ((force * 1) + 0.25f) * horizontalForceScale, ForceMode.Impulse);

        gunFiredEvent?.Invoke(new Vector3((force * 3) + 2, horizontalForceScale * 5.5f), Vector3.zero);

        recoilT += recoilIncreasPerBullet;
    }
}
