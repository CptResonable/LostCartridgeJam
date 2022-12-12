using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {
    [SerializeField] private float timeBetweenShots;
    [SerializeField] private Transform tMuzzle;

    [SerializeField] private GameObject goSFX_gunShot;
    [SerializeField] private GameObject prefabBullet;
    [SerializeField] private GameObject prefab_vfxMuzzleFlash;

    [SerializeField] private float muzzleVelocity;

    [SerializeField] private AnimationCurve recoilCurve;
    [SerializeField] private AnimationCurve horizontalRecoilCurve;
    [SerializeField] private float backRecoil;
    [SerializeField] private float upRecoil;
    [SerializeField] private float horizontalRecoil;
    [SerializeField] private float recoilIncreasPerBullet;
    [SerializeField] private float recoilResetRate;
    [SerializeField] private float recoilT;
    [SerializeField] private float headUpRecoil;
    [SerializeField] private float headHorizontalRecoil;

    [SerializeField] private Player player;

    private float cooldown = 0;

    public int bulletsInMagCount = 10;

    public bool hasSlideStop = true;

    public event Delegates.EmptyDelegate gunFiredEvent;
    public event Delegates.EmptyDelegate reloadFinishedEvent;

    private void Update() {
        if (cooldown <= 0) {
            if (Input.GetKey(KeyCode.Mouse0)) {
                cooldown = timeBetweenShots;
                Fire();
            }
            else {
                recoilT -= Time.deltaTime;
                if (recoilT < 0)
                    recoilT = 0;
            }
        }

        cooldown -= Time.deltaTime;
    }

    private void Fire() {
        Rigidbody rb = transform.parent.GetComponent<Rigidbody>();
        rb.AddForce(-transform.forward * 9, ForceMode.Impulse);
        rb.AddTorque(transform.right * -1.5f, ForceMode.Impulse);

        GameObject goBullet = EZ_Pooling.EZ_PoolManager.Spawn(prefabBullet.transform, tMuzzle.position, tMuzzle.rotation).gameObject;
        Bullet bullet = goBullet.GetComponent<Bullet>();
        bullet.Fire(tMuzzle.forward * muzzleVelocity);

        GameObject goMuzzle = EZ_Pooling.EZ_PoolManager.Spawn(prefab_vfxMuzzleFlash.transform, tMuzzle.position, tMuzzle.rotation).gameObject;
        Vfx_muzzleFlash muzzleFlash = goMuzzle.GetComponent<Vfx_muzzleFlash>();
        muzzleFlash.Initiate();
    }
}
