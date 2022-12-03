using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FPCamera {
    public Transform tHead;

    public float yaw, pitch;

    private Player player;
    public void Initialize(Player player) {
        this.player = player;

        player.updateEvent += player_updateEvent;
        player.fixedUpdateEvent += Player_fixedUpdateEvent;
    }

    private void Player_fixedUpdateEvent() {
    }

    public void player_updateEvent() {
        pitch -= Input.GetAxis("Mouse Y") * Settings.MOUSE_SENSITIVITY;
        yaw += Input.GetAxis("Mouse X") * Settings.MOUSE_SENSITIVITY;

        pitch = Mathf.Clamp(pitch, -89, 89);
        tHead.rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    public void SetRotation() {
        tHead.rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    public void Recoil(float pitch, float yaw) {
        this.pitch += pitch;
        this.yaw += yaw;
        this.pitch = Mathf.Clamp(pitch, -89, 89);
    }
}
