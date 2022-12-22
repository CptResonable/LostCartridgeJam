using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Arm {
    [SerializeField] private Transform tBase;
    [SerializeField] private Transform tHandTarget;

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
        tBase.position = character.fpCamera.tHead.position;
        tBase.rotation = Quaternion.Lerp(tBase.rotation, character.fpCamera.tHead.rotation, Time.fixedDeltaTime * 20);

        handRotationOffset = new Vector3(handRotationOffset.x, handRotationOffset.y, Mathf.Lerp(handRotationOffset.z, -character.characterInput.moveInput.x * 25 + character.rb.angularVelocity.y * 200, Time.deltaTime * 8));

        if (character.weaponController.equipedGun != null)
            tHandTarget.localPosition = Vector3.Lerp(character.weaponController.equipedGun.targetHandPosition, character.weaponController.equipedGun.targetAdsHandPosition, hipAdsInterpolator.t);
        tHandTarget.rotation = character.fpCamera.tHead.rotation;
        tHandTarget.Rotate(handRotationOffset);
        if (reloadSpinPitch != 0) {
            tHandTarget.Rotate(new Vector3(0, 0, -50), Space.Self);
            tHandTarget.Rotate(new Vector3(-reloadSpinPitch, 0, 0), Space.Self);
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
