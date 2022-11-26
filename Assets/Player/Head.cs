using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Head {
    public Transform tHead;

    private float yaw, pitch;

    public void Initialize(Player player) {
        player.updateEvent += player_updateEvent;
    }

    public void player_updateEvent() {
        pitch -= Input.GetAxis("Mouse Y") * Settings.MOUSE_SENSITIVITY * Time.deltaTime;
        yaw += Input.GetAxis("Mouse X") * Settings.MOUSE_SENSITIVITY * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, -89, 89);
        tHead.parent.rotation = Quaternion.Euler(0, yaw, 0);
        tHead.localRotation = Quaternion.Euler(pitch, 0, 0);
    }

    public void Recoil(float pitch, float yaw) {
        this.pitch += pitch;
        this.yaw += yaw;
        pitch = Mathf.Clamp(pitch, -89, 89);
    }
}
