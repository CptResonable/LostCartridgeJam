using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInput : MonoBehaviour {
    public KeyAction action_moveForward;
    public KeyAction action_moveBackward;
    public KeyAction action_moveLeft;
    public KeyAction action_moveRight;
    public KeyAction action_jump;
    public KeyAction action_attack;
    public KeyAction action_ads;
    public KeyAction action_reload;

    protected List<KeyAction> actions = new List<KeyAction>();

    public MouseMovement mouseMovement;
    public Vector3 moveInput;

    public void Init() {
        actions.Add(action_moveForward);
        actions.Add(action_moveBackward);
        actions.Add(action_moveLeft);
        actions.Add(action_moveRight);
        actions.Add(action_jump);
        actions.Add(action_attack);
        actions.Add(action_ads);
        actions.Add(action_reload);
    }
}

[System.Serializable]
public class KeyAction {
    public KeyCode keyCode;
    public bool isDown;
    public event Delegates.EmptyDelegate keyDownEvent;
    public event Delegates.EmptyDelegate keyUpEvent;

    public void InitPlayer(Player player) {
        player.updateEvent += Player_updateEvent;
    }

    public void InitNPC() {

    }

    private void Player_updateEvent() {

        if (Input.GetKeyDown(keyCode)) {
            isDown = true;
            keyDownEvent?.Invoke();
        }

        if (Input.GetKeyUp(keyCode)) {
            isDown = false;
            keyUpEvent?.Invoke();
        }
    }
}

public class MouseMovement {
    public float x, y;
    public float xDelta, yDelta;

    public MouseMovement(Player player) {
        player.updateEvent += Player_updateEvent;
    }

    public MouseMovement(NPC npc) {
        npc.updateEvent += Npc_updateEvent;
    }

    private void Player_updateEvent() {
        xDelta = Input.GetAxis("Mouse X");
        yDelta = Input.GetAxis("Mouse Y");
    }

    private void Npc_updateEvent() {
    }
}