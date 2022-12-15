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

    public void Init(Character character) {
        this.character = character;

        character.updateEvent += Character_updateEvent;

        character.characterInput.action_ads.keyDownEvent += Action_ads_keyDownEvent;
        character.characterInput.action_ads.keyUpEvent += Action_ads_keyUpEvent;
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
