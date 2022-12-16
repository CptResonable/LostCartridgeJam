using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponController {
    public Gun equipedGun;

    private Character character;

    public bool isADS;

    public event Delegates.EmptyDelegate adsEnteredEvent;
    public event Delegates.EmptyDelegate adsExitedEvent;
    public event Delegates.FloatDelegate reloadStartedEvent;

    public void Init(Character character) {
        this.character = character;

        character.updateEvent += Character_updateEvent;

        character.characterInput.action_ads.keyDownEvent += Action_ads_keyDownEvent;
        character.characterInput.action_ads.keyUpEvent += Action_ads_keyUpEvent;

        character.characterInput.action_reload.keyDownEvent += Action_reload_keyDownEvent;

        if (equipedGun != null)
            equipedGun.reloadStartedEvent += EquipedGun_reloadStartedEvent;
    }

    private void EquipedGun_reloadStartedEvent(float reloadTime) {
        reloadStartedEvent?.Invoke(reloadTime);
    }

    private void Action_reload_keyDownEvent() {
        Debug.Log("RELOAD!");
        equipedGun.Reload();
    }

    private void Character_updateEvent() {
        if (character.characterInput.action_attack.isDown)
            equipedGun.TryFire();
    }

    private void Action_ads_keyUpEvent() {
        isADS = false;
        adsExitedEvent?.Invoke();
    }

    private void Action_ads_keyDownEvent() {
        isADS = true;
        adsEnteredEvent?.Invoke();
    }
}
