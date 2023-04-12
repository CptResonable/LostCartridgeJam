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

    public void Init(Character character) {
        this.character = character;

        hand_R.Init(character);
        hand_L.Init(character);
        hand_L.rb.inertiaTensor = hand_R.rb.inertiaTensor;

        EvalulateTargetAnimationWeight();

        character.fixedUpdateEvent += Player_fixedUpdateEvent;
        character.lateUpdateEvent += Character_lateUpdateEvent;

        character.equipmentManager.itemEquipedEvent += EquipmentManager_itemEquipedEvent;
        character.equipmentManager.itemUnequipedEvent += EquipmentManager_itemUnequipedEvent;

        character.locomotion.sprintStartedEvent += Locomotion_sprintStartedEvent;
        character.locomotion.sprintEndedEvent += Locomotion_sprintEndedEvent;
        character.locomotion.wallrunController.verticalRunStarted += WallrunController_verticalRunStarted;
        character.locomotion.wallrunController.verticalRunStopped += WallrunController_verticalRunStopped;
        character.locomotion.wallrunController.horizontalRunStarted += WallrunController_horizontalRunStarted;
        character.locomotion.wallrunController.horizontalRunStopped += WallrunController_horizontalRunStopped;
    }

    private void Player_fixedUpdateEvent() {
        tArmControllerBase.position = character.fpCamera.tCamera.position;
        tArmControllerBase.rotation = Quaternion.Lerp(tArmControllerBase.rotation, character.fpCamera.tCamera.rotation, Time.fixedDeltaTime * 12); // Lerp towards camera forward
    }

    private void Character_lateUpdateEvent() {
        animationWeight = Mathf.Lerp(1, animationWeightInterpolator.t, character.equipmentManager.unequipEquipInterpolator.t);
    }

    private void EvalulateTargetAnimationWeight() {
        //if (hipAdsInterpolationCorutine != null)
        //    character.StopCoroutine(hipAdsInterpolationCorutine);

        if (character.locomotion.state_grounded.isSprinting || character.locomotion.wallrunController.isWallRunning || character.equipmentManager.state == CharacterEquipmentManager.State.nothingEquiped) {
            animationWeightCorutine = character.StartCoroutine(InterpolationUtils.i.SmoothStep(animationWeightInterpolator.t, 1, 4, animationWeightInterpolator));
        }
        else {
            animationWeightCorutine = character.StartCoroutine(InterpolationUtils.i.SmoothStep(animationWeightInterpolator.t, 0, 4, animationWeightInterpolator));
        }
    }

    private void EquipmentManager_itemUnequipedEvent(Equipment item) {
        EvalulateTargetAnimationWeight();
    }

    private void EquipmentManager_itemEquipedEvent(Equipment item) {
        EvalulateTargetAnimationWeight();
    }


    private void Locomotion_sprintStartedEvent() {
        EvalulateTargetAnimationWeight();
    }

    private void Locomotion_sprintEndedEvent() {
        EvalulateTargetAnimationWeight();
    }

    private void WallrunController_verticalRunStarted() {
        EvalulateTargetAnimationWeight();
    }

    private void WallrunController_verticalRunStopped() {
        EvalulateTargetAnimationWeight();
    }

    private void WallrunController_horizontalRunStarted() {
        //EvalulateTargetAnimationWeight();
    }

    private void WallrunController_horizontalRunStopped() {
        //EvalulateTargetAnimationWeight();
    }

}
