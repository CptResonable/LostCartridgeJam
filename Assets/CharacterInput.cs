using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInput : MonoBehaviour {
    public MouseMovement mouseMovement;

    public class MouseMovement {
        public float x, y;
        public float xDelta, yDelta;
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

    public void InitNPC(Player player) {

    }

    private void Player_updateEvent() {

        if (Input.GetKeyDown(keyCode)) {
            isDown = false;
            keyDownEvent?.Invoke();
        }

        if (Input.GetKeyUp(keyCode)) {
            isDown = false;
            keyUpEvent?.Invoke();
        }
    }
}