using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FPCamera {
    public Transform tHead;

    public float yaw, pitch;

    private Character character;
    public void Initialize(Character character) {
        this.character = character;

        character.updateEvent += character_updateEvent;
        character.fixedUpdateEvent += character_fixedUpdateEvent;
    }

    private void character_fixedUpdateEvent() {
    }

    public void character_updateEvent() {
        pitch -= character.characterInput.mouseMovement.yDelta * Settings.MOUSE_SENSITIVITY;
        yaw += character.characterInput.mouseMovement.xDelta * Settings.MOUSE_SENSITIVITY;

        pitch = Mathf.Clamp(pitch, -89, 89);
        tHead.parent.rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    public void SetRotation() {
        tHead.parent.rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    public void Recoil(float pitch, float yaw) {
        this.pitch += pitch;
        this.yaw += yaw;
        this.pitch = Mathf.Clamp(pitch, -89, 89);
    }
}
