using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponController {
    public Gun equipedGun;

    private Character character;

    public void Init(Character character) {
        this.character = character;

        character.updateEvent += Character_updateEvent;
    }

    private void Character_updateEvent() {
        if (character.characterInput.action_attack.isDown)
            equipedGun.TryFire();
    }
}
