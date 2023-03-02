using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

[System.Serializable]
public class Arms {
    public RightHand hand_R;
    public LeftHand hand_L;

    [SerializeField] private Transform tArmControllerBase; // used as a base to move hand targets for aiming guns etc

    private Character character;

    // Inerpolation between straight animation and IK
    private TWrapper animationWeightInterpolator = new TWrapper(0, 1, 0);
    public float animationWeight;
    private Coroutine animationWeightCorutine;

    // Inerpolation between hip and ads hand positions
    public TWrapper hipAdsInterpolator = new TWrapper(0, 1, 0);
    private Coroutine hipAdsInterpolationCorutine;

    public void Init(Character character) {
        this.character = character;

        hand_R.Init(character);
        hand_L.Init(character);
        hand_L.rb.inertiaTensor = hand_R.rb.inertiaTensor;

        character.fixedUpdateEvent += Player_fixedUpdateEvent;
        character.lateUpdateEvent += Character_lateUpdateEvent;

        character.weaponController.adsEnteredEvent += WeaponController_adsEnteredEvent;
        character.weaponController.adsExitedEvent += WeaponController_adsExitedEvent;

        character.locomotion.sprintStartedEvent += Locomotion_sprintStartedEvent;
        character.locomotion.sprintEndedEvent += Locomotion_sprintEndedEvent;
        character.locomotion.wallrunController.verticalRunStarted += WallrunController_verticalRunStarted;
        character.locomotion.wallrunController.verticalRunStopped += WallrunController_verticalRunStopped;
    }

    private void Player_fixedUpdateEvent() {
        tArmControllerBase.position = character.fpCamera.tCameraTarget.position;
        tArmControllerBase.rotation = Quaternion.Lerp(tArmControllerBase.rotation, character.fpCamera.tCamera.rotation, Time.fixedDeltaTime * 12); // Lerp towards camera forward
    }

    private void Character_lateUpdateEvent() {
        animationWeight = Mathf.Lerp(animationWeightInterpolator.t, 1, character.weaponController.weaponSwapAnimationThing);
    }

    private void EvanulateTargetAnimationWeight() {
        if (hipAdsInterpolationCorutine != null)
            character.StopCoroutine(hipAdsInterpolationCorutine);

        if (character.locomotion.state_grounded.isSprinting || character.locomotion.wallrunController.isWallClimbing) {
            animationWeightCorutine = character.StartCoroutine(InterpolationUtils.i.SmoothStep(animationWeightInterpolator.t, 1, 4, animationWeightInterpolator));
        }
        else {
            animationWeightCorutine = character.StartCoroutine(InterpolationUtils.i.SmoothStep(animationWeightInterpolator.t, 0, 4, animationWeightInterpolator));
        }
    }

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

    private void Locomotion_sprintStartedEvent() {
        EvanulateTargetAnimationWeight();
    }

    private void Locomotion_sprintEndedEvent() {
        EvanulateTargetAnimationWeight();
    }

    private void WallrunController_verticalRunStarted() {
        EvanulateTargetAnimationWeight();
    }

    private void WallrunController_verticalRunStopped() {
        EvanulateTargetAnimationWeight();
    }
}
