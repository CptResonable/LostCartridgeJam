using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GunSpecs", menuName = "ScriptableObjects/GunSpecs")]
public class GunSpecs : ScriptableObject {
    public bool isAuto;
    public int magSize;
    public float muzzleVelocity;
    public float reloadTime;
    public float damage;
    public int rpm;

    [Header("Recoil")]
    public AnimationCurve recoilCurve;
    public AnimationCurve horizontalRecoilCurve;
    public AnimationCurve recoilResetSpeedCurve;
    public float backRecoil;
    public float upRecoil;
    public float horizontalRecoil;
    public float recoilIncreasPerBullet;
    public float recoilResetRate;
    public float recoilT;
    public float headUpRecoil;
    public float headHorizontalRecoil;
    public float horizontalChangeSpeed;
}
