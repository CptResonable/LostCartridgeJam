using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : CharacterInput {
    public override void Init(Character character) {
        base.Init(character);

        mouseMovement = new MouseMovement(character);

        foreach (KeyAction action in actions) {
            action.Init(character);
        }

        character.updateEvent += Character_updateEvent;
    }

    private void Character_updateEvent() {
        moveInput = Vector3.zero;

        if (action_moveForward.isDown)
            moveInput.z += 1;
        if (action_moveBackward.isDown)
            moveInput.z -= 1;
        if (action_moveLeft.isDown)
            moveInput.x -= 1;
        if (action_moveRight.isDown)
            moveInput.x += 1;

        moveInput.Normalize();
    }
}
