using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

[System.Serializable]
public class Arm {
    [SerializeField] private Transform tBase;
    [SerializeField] private Transform tHandTarget_R;
    [SerializeField] private Transform tHandTarget_L;
    [SerializeField] private Transform tWeaponTarget_R;
    [SerializeField] private Transform tIkTarget_L;
    [SerializeField] private Transform tIkTarget_R;

    [SerializeField] private LimbIK armIK_R;
    [SerializeField] private IK armIK_L;
    //[SerializeField] private Transform fuck;
    [SerializeField] private Vector3 ehh;

    //public Vector3 hipHandPosition;
    //public Vector3 adsHandPosition;

    private Character character;

    public Vector3 handRotationOffset;

    public TWrapper hipAdsInterpolator = new TWrapper(0, 1, 0);
    private Coroutine interpolationCorutine;
    private Coroutine reloadCorutine;

    public void Init(Character character) {
        this.character = character;

        //hipHandPosition = tHandTarget.localPosition;

        character.fixedUpdateEvent += Player_fixedUpdateEvent;

        character.weaponController.adsEnteredEvent += WeaponController_adsEnteredEvent;
        character.weaponController.adsExitedEvent += WeaponController_adsExitedEvent;
        character.weaponController.reloadStartedEvent += WeaponController_reloadStartedEvent;
        character.weaponController.weaponEquipedEvent += WeaponController_weaponEquipedEvent;
    }

    private void Player_fixedUpdateEvent() {
        WeaponHandUpdate();

        if (character.playerController.isSprining) {
            tHandTarget_R.position = character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandR).position;
            tHandTarget_R.rotation = character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandR).rotation;
            tHandTarget_L.position = character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandL).position;
            tHandTarget_L.rotation = character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandL).rotation;
        }
        else {
            tHandTarget_R.position = tWeaponTarget_R.position;
            tHandTarget_R.rotation = tWeaponTarget_R.rotation;
            tHandTarget_L.position = tWeaponTarget_R.position;
            tHandTarget_L.rotation = tWeaponTarget_R.rotation;
        }
    }

    private void WeaponHandUpdate() {
        tBase.position = character.fpCamera.tCamera.position;
        tBase.rotation = Quaternion.Lerp(tBase.rotation, character.fpCamera.tCamera.rotation, Time.fixedDeltaTime * 12);

        //handRotationOffset = new Vector3(handRotationOffset.x, Mathf.Lerp(handRotationOffset.z, -character.characterInput.moveInput.x * 25 + character.rb.angularVelocity.y * -2, Time.deltaTime * 8), handRotationOffset.z);
        handRotationOffset = new Vector3(handRotationOffset.x, handRotationOffset.y, Mathf.Lerp(handRotationOffset.z, -character.characterInput.moveInput.x * 25 + character.rb.angularVelocity.y * -2, Time.fixedDeltaTime * 8));

        if (character.weaponController.equipedGun != null)
            tWeaponTarget_R.localPosition = Vector3.Lerp(character.weaponController.equipedGun.targetHandPosition, character.weaponController.equipedGun.targetAdsHandPosition, hipAdsInterpolator.t);
        tWeaponTarget_R.rotation = character.fpCamera.tCamera.rotation;
        tWeaponTarget_R.Rotate(handRotationOffset);
        tWeaponTarget_R.Rotate(ehh);
        //tWeaponTarget_R.Rotate(new Vector3(-90, 0, 180));

        if (reloadSpinPitch != 0) {
            tWeaponTarget_R.Rotate(new Vector3(0, 0, -50), Space.Self);
            tWeaponTarget_R.Rotate(new Vector3(-reloadSpinPitch, 0, 0), Space.Self);
        }
    }

    private void WeaponController_adsEnteredEvent() {
        if (interpolationCorutine != null)
            character.StopCoroutine(interpolationCorutine);

        interpolationCorutine = character.StartCoroutine(InterpolationUtils.i.SmoothStep(hipAdsInterpolator.t, 1, 4, hipAdsInterpolator));
    }

    private void WeaponController_adsExitedEvent() {
        if (interpolationCorutine != null)
            character.StopCoroutine(interpolationCorutine);

        interpolationCorutine = character.StartCoroutine(InterpolationUtils.i.SmoothStep(hipAdsInterpolator.t, 0, 4, hipAdsInterpolator));
    }

    private void WeaponController_reloadStartedEvent(float reloadTime) {
        reloadCorutine = character.StartCoroutine(ReloadCorutine(reloadTime));
    }

    private void WeaponController_weaponEquipedEvent() {
        if (reloadCorutine != null)
            character.StopCoroutine(reloadCorutine);

        reloadSpinPitch = 0;
    }

    private float reloadSpinPitch;
    private IEnumerator ReloadCorutine(float reloadTime) {
        float t = 0;

        while (t < 1) {
            t += Time.deltaTime / reloadTime;
            reloadSpinPitch = 1440 * t;
            yield return new WaitForEndOfFrame();
        }

        reloadSpinPitch = 0;
    }
}
