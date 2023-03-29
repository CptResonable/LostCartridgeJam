using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StanceController {
    public bool isADS;

    // Inerpolation between hip and ads. Used for zoom amount and hand positions etc
    public TWrapper hipAdsInterpolator = new TWrapper(0, 1, 0);
    private Coroutine hipAdsInterpolationCorutine;

    // Modifies upper body rotation when item is equiped
    public UpperBody.UpperBodyRotationModifier upperBodyRotationModifier_weaponYaw = new UpperBody.UpperBodyRotationModifier(Vector3.zero, new Vector3(0, 30, 0));

    public Delegates.EmptyDelegate enterAdsEvent;
    public Delegates.EmptyDelegate exitAdsEvent;

    private Character character;
    public void Init(Character character) {
        this.character = character;

        character.upperBody.AddModifier(upperBodyRotationModifier_weaponYaw);

        character.updateEvent += Character_updateEvent;

        character.equipmentManager.itemEquipedEvent += EquipmentManager_itemEquipedEvent;
        character.equipmentManager.itemUnequipedEvent += EquipmentManager_itemUnequipedEvent;

        character.locomotion.stateChangedEvent += Locomotion_stateChangedEvent;
        character.locomotion.sprintStartedEvent += Locomotion_sprintStartedEvent;

        character.characterInput.action_ads.keyDownEvent += Action_ads_keyDownEvent;
        character.characterInput.action_ads.keyUpEvent += Action_ads_keyUpEvent;
    }

    private void Character_updateEvent() {

        // Rotate torso 2
        if (character.equipmentManager.state == CharacterEquipmentManager.State.gunEquiped && character.locomotion.activeStateEnum == Locomotion.LocomotionState.StateIDEnum.Grounded && !character.locomotion.state_grounded.isSprinting && !character.locomotion.state_grounded.isSliding)
            upperBodyRotationModifier_weaponYaw.bonusEuler_torso2 = Vector3.Lerp(upperBodyRotationModifier_weaponYaw.bonusEuler_torso2, new Vector3(0, 30, 0), Time.deltaTime * 4);
        else
            upperBodyRotationModifier_weaponYaw.bonusEuler_torso2 = Vector3.Lerp(upperBodyRotationModifier_weaponYaw.bonusEuler_torso2, new Vector3(0, 0, 0), Time.deltaTime * 4);
    }

    private void EquipmentManager_itemEquipedEvent(Equipment item) {
    }

    private void EquipmentManager_itemUnequipedEvent(Equipment item) {
        if (isADS)
            ExitAds();
    }

    private void Locomotion_stateChangedEvent(Locomotion.LocomotionState newState) {
        if (isADS) {
            if (!newState.canADS)
                ExitAds();
        }
    }

    private void Locomotion_sprintStartedEvent() {
        if (isADS)
            ExitAds();
    }

    private void Action_ads_keyDownEvent() {
        if (!isADS)
            EnterAds();
    }

    private void Action_ads_keyUpEvent() {
        if (isADS)
            ExitAds();
    }

    private void EnterAds() {

        // Make sure ADS is possible
        if (!character.locomotion.activeState.canADS || character.locomotion.state_grounded.isSprinting)
            return;

        isADS = true;

        if (hipAdsInterpolationCorutine != null)
            character.StopCoroutine(hipAdsInterpolationCorutine);

        hipAdsInterpolationCorutine = character.StartCoroutine(InterpolationUtils.i.SmoothStep(hipAdsInterpolator.t, 1, 4, hipAdsInterpolator));

        enterAdsEvent?.Invoke();
    }

    private void ExitAds() {
        isADS = false;

        if (hipAdsInterpolationCorutine != null)
            character.StopCoroutine(hipAdsInterpolationCorutine);

        hipAdsInterpolationCorutine = character.StartCoroutine(InterpolationUtils.i.SmoothStep(hipAdsInterpolator.t, 0, 4, hipAdsInterpolator));

        exitAdsEvent?.Invoke();
    }
}
