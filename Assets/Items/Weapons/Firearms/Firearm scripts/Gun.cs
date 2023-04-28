using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Equipment {
    [Header("--- GUN ---")]

    public GunSpecs specs;

    [SerializeField] private float timeBetweenShots;
    [SerializeField] private Transform tMuzzle;
    [SerializeField] private Transform tMag;

    [SerializeField] private GameObject prefabBullet;
    [SerializeField] private GameObject prefab_vfxMuzzleFlash;

    [SerializeField] public bool isAuto;
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

    public GunAnimationController gunAnimationController;

    public bool consumeAmmo;
    public int ammoReserve;

    private float cooldown = 0;
    private float timeSinceLastShot = 0;

    public bool bulletInChaimber = true;
    public bool isReloading = false;
    public bool magIn = true;
    public int bulletsInMagCount = 30;
    private Coroutine reloadCorutine;
    //public bool isReloading = false;

    public bool hasSlideStop = true;

    public delegate void GunFiredDelegate(Vector3 rotationalRecoil, Vector3 translationalRecoil);
    public event GunFiredDelegate gunFiredEvent;
    public event Delegates.FloatDelegate reloadStartedEvent;
    public event Delegates.EmptyDelegate reloadFinishedEvent;
    public event Delegates.EmptyDelegate bulletChaimeredEvent;

    protected override void Awake() {
        base.Awake();

        gunAnimationController = GetComponentInChildren<GunAnimationController>();

        gunAnimationController.magInsertedEvent += GunAnimationController_magInsertedEvent;
        gunAnimationController.magDroppedEvent += GunAnimationController_magDroppedEvent;
        gunAnimationController.boltRackedEvent += GunAnimationController_boltRackedEvent;
    }

    public override void Equip(Character character) {
        base.Equip(character);
        character.updateEvent += Character_updateEvent;
        character.characterInput.action_attack.keyDownEvent += Action_attack_keyDownEvent;
        character.characterInput.action_reload.keyDownEvent += Action_reload_keyDownEvent;
    }

    private void Action_reload_keyDownEvent() {
        Reload();
    }

    private void Action_attack_keyDownEvent() {
        TryFire(true);
    }

    public override void Unequip() {
        ReloadCanceled();
        character.updateEvent -= Character_updateEvent;
        character.characterInput.action_attack.keyDownEvent -= Action_attack_keyDownEvent;
        character.characterInput.action_reload.keyDownEvent -= Action_reload_keyDownEvent;

        base.Unequip();
    }

    private void Character_updateEvent() {
        if (character == null)
            return;

        // Automatic fire
        if (isAuto) {
            if (character.characterInput.action_attack.isDown)
                TryFire(false);
        }

        transform.parent.GetComponent<Rigidbody>().inertiaTensor = new Vector3(0.0168549232f, 0.00615772139f, 0.0125291189f);
        transform.parent.GetComponent<Rigidbody>().inertiaTensor = Vector3.Lerp(new Vector3(0.0873092264f, 0.012772995f, 0.0763731599f), new Vector3(0.0168549232f, 0.00615772139f, 0.0125291189f), 0.5f);


    }

    protected override void LateUpdate() {
        base.LateUpdate();

        cooldown -= Time.deltaTime;

        recoilT -= Time.deltaTime * recoilResetSpeedCurve.Evaluate(timeSinceLastShot);
        timeSinceLastShot += Time.deltaTime;

        if (recoilT < 0)
            recoilT = 0;
    }

    public void TryFire(bool triggerPressedThisFrame) {
        if (cooldown <= 0) {
            if (bulletInChaimber) {
                cooldown = timeBetweenShots;
                Fire();
            }
            else if (triggerPressedThisFrame) {           
                AudioManager.i.PlaySoundStatic(AudioManager.i.sfxLibrary.dryFire_01, transform.position);
            }
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

        isReloading = true;
        reloadStartedEvent?.Invoke(reloadTime);

        gunAnimationController.InitReload();
    }

    public void ReloadCanceled() {
        isReloading = false;
    }

    private void Fire() {
        timeSinceLastShot = 0;

        bulletInChaimber = false;

        Recoil();

        ProjectileParams projectileParams = new ProjectileParams(tMuzzle.position, tMuzzle.forward, damage, character.ID);
        //ProjectileManager.i.

        GameObject goBullet = EZ_Pooling.EZ_PoolManager.Spawn(prefabBullet.transform, tMuzzle.position - tMuzzle.forward * 0.05f, tMuzzle.rotation).gameObject;
        Bullet bullet = goBullet.GetComponent<Bullet>();
        bullet.Fire(character, tMuzzle.forward * muzzleVelocity, damage);

        // VFX
        GameObject goMuzzle = EZ_Pooling.EZ_PoolManager.Spawn(prefab_vfxMuzzleFlash.transform, tMuzzle.position, tMuzzle.rotation).gameObject;
        Vfx_muzzleFlash muzzleFlash = goMuzzle.GetComponent<Vfx_muzzleFlash>();
        muzzleFlash.Initiate(tMuzzle);

        // Chaimber new bullet
        if (bulletsInMagCount > 0) {
            ChaimerBullet();
        }
    }

    private void ChaimerBullet() {
        bulletsInMagCount--;
        bulletInChaimber = true;

        bulletChaimeredEvent?.Invoke();
    }

    #region Animation events
    private void GunAnimationController_boltRackedEvent() {
        ChaimerBullet();

        if (isReloading)
            isReloading = false;
    }

    private void GunAnimationController_magDroppedEvent() {
        magIn = false;
        bulletsInMagCount = 0;
        tMag.gameObject.SetActive(false);
    }

    private void GunAnimationController_magInsertedEvent() {
        tMag.gameObject.SetActive(true);

        if (consumeAmmo && ammoReserve < magSize) {
            bulletsInMagCount = ammoReserve;
            ammoReserve = 0;
        }
        else {
            ammoReserve -= (magSize - bulletsInMagCount);
            bulletsInMagCount = magSize;
        }

        magIn = true;

        if (bulletInChaimber)
            isReloading = false;
    }
    #endregion

    private void Recoil() {
        if (isAuto) {
            Rigidbody rb = transform.parent.GetComponent<Rigidbody>();

            float force = recoilCurve.Evaluate(recoilT);
            float horizontalForceScale = (Mathf.PerlinNoise(recoilT * horizontalChangeSpeed, 321.43f) - 0.2f) * 2 * horizontalRecoilCurve.Evaluate(recoilT);

            rb.AddForce(-tMuzzle.forward * ((force * 5) + 5), ForceMode.Impulse);
            rb.AddForce(tMuzzle.right * horizontalForceScale * 4, ForceMode.Impulse);
            rb.AddTorque(tMuzzle.right * -((force * 2f) + 0.0f), ForceMode.Impulse);
            rb.AddTorque(tMuzzle.up * horizontalForceScale * 1.25f, ForceMode.Impulse);

            gunFiredEvent?.Invoke(new Vector3((force * 3) + 2, horizontalForceScale * 5.5f), Vector3.zero);

            recoilT += recoilIncreasPerBullet;
        }
        else {
            Rigidbody rb = transform.parent.GetComponent<Rigidbody>();

            float force = recoilCurve.Evaluate(recoilT);
            float horizontalForceScale = (Mathf.PerlinNoise(recoilT * horizontalChangeSpeed, 321.43f) - 0.2f) * 2 * horizontalRecoilCurve.Evaluate(recoilT);
            rb.AddForce(-tMuzzle.forward * ((force * 5) + 4), ForceMode.Impulse);
            rb.AddForce(tMuzzle.right * (horizontalForceScale * 4 + 0.2f), ForceMode.Impulse);
            rb.AddTorque(tMuzzle.right * -((force * 1f) + 0.7f), ForceMode.Impulse);
            rb.AddTorque(tMuzzle.up * ((horizontalForceScale * 0.5f) + Random.Range(-0.1f, 0.1f)), ForceMode.Impulse);

            gunFiredEvent?.Invoke(new Vector3((force * 6) + 4, horizontalForceScale * 10.5f), Vector3.zero);

            recoilT += recoilIncreasPerBullet;
        }
    }
}
