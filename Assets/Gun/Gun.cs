using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {
    [SerializeField] private float timeBetweenShots;
    [SerializeField] private Transform tMuzzle;

    [SerializeField] private GameObject goSFX_gunShot;
    [SerializeField] private GameObject prefabBullet;

    [SerializeField] private float muzzleVelocity;

    [SerializeField] private Transform tRecoil;
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

    private Vector3 targetLocalPosition;
    private Vector3 targetLocalRotation;

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

        //targetLocalPosition = Vector3.Lerp(targetLocalPosition, Vector3.zero, 10 * Time.deltaTime);
        //targetLocalRotation = Vector3.Lerp(targetLocalRotation, Vector3.zero, 10 * Time.deltaTime);
        //tRecoil.localPosition = Vector3.Lerp(tRecoil.localPosition, targetLocalPosition, 100 * Time.deltaTime);
        //tRecoil.localRotation = Quaternion.Lerp(tRecoil.localRotation, Quaternion.Euler(targetLocalRotation), 100 * Time.deltaTime);
        //tRecoil.localPosition = targetLocalPosition;
    }

    private void Fire() {
        Rigidbody rb = transform.parent.GetComponent<Rigidbody>();
        rb.AddForce(-transform.forward * 4, ForceMode.Impulse);
        //rb.AddForce(transform.parent.up * 300);
        rb.AddTorque(transform.right * -5, ForceMode.Impulse);
        //rb.AddTorque(Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized * -5, ForceMode.Impulse);
    }

    //private void Fire() {
    //    //Instantiate(goSFX_gunShot, transform);
    //    GameObject goBullet = Instantiate(prefabBullet);
    //    goBullet.transform.position = tMuzzle.position;
    //    Bullet bullet = goBullet.GetComponent<Bullet>();
    //    bullet.Fire(tMuzzle.forward * muzzleVelocity);

    //    float t = recoilCurve.Evaluate(recoilT);
    //    targetLocalPosition += Vector3.back * backRecoil;
    //    targetLocalRotation -= Vector3.right * t * upRecoil;

    //    float horizontalRecoilDirThing = horizontalRecoilCurve.Evaluate(recoilT);
    //    targetLocalRotation += Vector3.up * t * horizontalRecoilDirThing * horizontalRecoil;

    //    recoilT += recoilIncreasPerBullet;

    //    player.head.Recoil(headUpRecoil * t, headHorizontalRecoil * t * horizontalRecoilDirThing);
    //}

    //private IEnumerator RecoilCorutine() {
    //    while (true) {

    //    }
    //}
}
