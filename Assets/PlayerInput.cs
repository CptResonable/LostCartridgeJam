using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : CharacterInput {
    public void Init(Player player) {
        base.Init();

        mouseMovement = new MouseMovement(player);

        foreach (KeyAction action in actions) {
            action.InitPlayer(player);
        }

        player.updateEvent += Player_updateEvent;
    }

    private void Player_updateEvent() {
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
