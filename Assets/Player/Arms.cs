using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

[System.Serializable]
public class Arms {
    public RightHand hand_R;
    public LeftHand hand_L;
    [SerializeField] private Transform tBase;
    //[SerializeField] private Transform tHandTarget_R;
    //[SerializeField] private Transform tHandTarget_L;
    //[SerializeField] private Transform tWeaponTarget_R;
    //[SerializeField] private Transform tIkTarget_L;
    //[SerializeField] private Transform tIkTarget_R;

    private Character character;

    public Vector3 handRotationOffset;

    // Inerpolation between straight animation and IK
    public TWrapper animationWeightInterpolator = new TWrapper(0, 1, 0);
    private Coroutine animationWeightCorutine;

    // Inerpolation between hip and ads hand positions
    public TWrapper hipAdsInterpolator = new TWrapper(0, 1, 0);
    private Coroutine hipAdsInterpolationCorutine;

    private Coroutine reloadCorutine;

    public void Init(Character character) {
        this.character = character;

        hand_R.Init(character);
        hand_L.Init(character);
        hand_L.rb.inertiaTensor = hand_R.rb.inertiaTensor;

        character.fixedUpdateEvent += Player_fixedUpdateEvent;

        character.weaponController.adsEnteredEvent += WeaponController_adsEnteredEvent;
        character.weaponController.adsExitedEvent += WeaponController_adsExitedEvent;
        character.weaponController.reloadStartedEvent += WeaponController_reloadStartedEvent;
        character.weaponController.weaponEquipedEvent += WeaponController_weaponEquipedEvent;

        character.locomotion.sprintStartedEvent += Locomotion_sprintStartedEvent;
        character.locomotion.sprintEndedEvent += Locomotion_sprintEndedEvent;
    }

    private void Player_fixedUpdateEvent() {
        //WeaponHandUpdate();

        //if (character.locomotion.isSprinting) {
        //    tHandTarget_R.position = character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandR).position;
        //    tHandTarget_R.rotation = character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandR).rotation;
        //    tHandTarget_L.position = character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandL).position;
        //    tHandTarget_L.rotation = character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandL).rotation;
        //}
        //else {
        //    tHandTarget_R.position = tWeaponTarget_R.position;
        //    tHandTarget_R.rotation = tWeaponTarget_R.rotation;
        //    tHandTarget_L.position = tWeaponTarget_R.position;
        //    tHandTarget_L.rotation = tWeaponTarget_R.rotation;
        //}
        tBase.position = character.fpCamera.tCamera.position;
        tBase.rotation = Quaternion.Lerp(tBase.rotation, character.fpCamera.tCamera.rotation, Time.fixedDeltaTime * 12);

        hand_R.ManualFixedUpdate();
        hand_L.ManualFixedUpdate();
        //character.handMovement_R.DoUpdate();
        //character.handMovement_L.DoUpdate();
    }

    //private void WeaponHandUpdate() {
    //    tBase.position = character.fpCamera.tCamera.position;
    //    tBase.rotation = Quaternion.Lerp(tBase.rotation, character.fpCamera.tCamera.rotation, Time.fixedDeltaTime * 12);

    //    handRotationOffset = new Vector3(handRotationOffset.x, handRotationOffset.y, Mathf.Lerp(handRotationOffset.z, -character.characterInput.moveInput.x * 25 + character.rb.angularVelocity.y * -2, Time.fixedDeltaTime * 8));

    //    if (character.weaponController.equipedGun != null)
    //        tWeaponTarget_R.localPosition = Vector3.Lerp(character.weaponController.equipedGun.targetHandPosition, character.weaponController.equipedGun.targetAdsHandPosition, hipAdsInterpolator.t);

    //    tWeaponTarget_R.rotation = character.fpCamera.tCamera.rotation;
    //    tWeaponTarget_R.Rotate(handRotationOffset);
    //    tWeaponTarget_R.Rotate(new Vector3(-90, 0, 180));

    //    if (reloadSpinPitch != 0) {
    //        tWeaponTarget_R.Rotate(new Vector3(0, 0, -50), Space.Self);
    //        tWeaponTarget_R.Rotate(new Vector3(-reloadSpinPitch, 0, 0), Space.Self);
    //    }
    //}

    private void WeaponController_adsEnteredEvent() {
        if (hipAdsInterpolationCorutine != null)
            character.StopCoroutine(hipAdsInterpolationCorutine);

        hipAdsInterpolationCorutine = character.StartCoroutine(InterpolationUtils.i.SmoothStep(hipAdsInterpolator.t, 1, 4, hipAdsInterpolator));
    }

    private void WeaponController_adsExitedEvent() {
        if (hipAdsInterpolationCorutine != null)
            character.StopCoroutine(hipAdsInterpolationCorutine);

        hipAdsInterpolationCorutine = character.StartCoroutine(InterpolationUtils.i.SmoothStep(hipAdsInterpolator.t, 0, 4, hipAdsInterpolator));
    }

    private void WeaponController_weaponEquipedEvent() {
        if (reloadCorutine != null)
            character.StopCoroutine(reloadCorutine);

        reloadSpinPitch = 0;
    }

    private void Locomotion_sprintStartedEvent() {
        if (animationWeightCorutine != null)
            character.StopCoroutine(animationWeightCorutine);

        animationWeightCorutine = character.StartCoroutine(InterpolationUtils.i.SmoothStep(animationWeightInterpolator.t, 1, 4, animationWeightInterpolator));
    }

    private void Locomotion_sprintEndedEvent() {
        if (animationWeightCorutine != null)
            character.StopCoroutine(animationWeightCorutine);

        animationWeightCorutine = character.StartCoroutine(InterpolationUtils.i.SmoothStep(animationWeightInterpolator.t, 0, 4, animationWeightInterpolator));
    }

    private void WeaponController_reloadStartedEvent(float reloadTime) {
        reloadCorutine = character.StartCoroutine(ReloadCorutine(reloadTime));
    }
    [HideInInspector] private float reloadSpinPitch;
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
