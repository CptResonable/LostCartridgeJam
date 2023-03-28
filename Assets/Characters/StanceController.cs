using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StanceController {
    public bool isADS;

    // Inerpolation between hip and ads. Used for zoom amount and hand positions etc
    public TWrapper hipAdsInterpolator = new TWrapper(0, 1, 0);
    private Coroutine hipAdsInterpolationCorutine;

    public Delegates.EmptyDelegate enterAdsEvent;
    public Delegates.EmptyDelegate exitAdsEvent;

    private Character character;
    public void Init(Character character) {
        this.character = character;

        character.updateEvent += Character_updateEvent;

        character.equipmentManager.itemEquipedEvent += EquipmentManager_itemEquipedEvent;
        character.equipmentManager.itemUnequipedEvent += EquipmentManager_itemUnequipedEvent;

        character.locomotion.sprintStartedEvent += Locomotion_sprintStartedEvent;

        character.characterInput.action_ads.keyDownEvent += Action_ads_keyDownEvent;
        character.characterInput.action_ads.keyUpEvent += Action_ads_keyUpEvent;
    }

    private void Character_updateEvent() {
    }

    private void EquipmentManager_itemEquipedEvent(Equipment item) {
    }

    private void EquipmentManager_itemUnequipedEvent() {
        if (isADS)
            ExitAds();
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
