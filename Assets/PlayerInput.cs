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

        if (Input.GetKey(KeyCode.W))
            moveInput.z += 1;
        if (Input.GetKey(KeyCode.S))
            moveInput.z -= 1;
        if (Input.GetKey(KeyCode.A))
            moveInput.x -= 1;
        if (Input.GetKey(KeyCode.D))
            moveInput.x += 1;

        moveInput.Normalize();
    }
}
