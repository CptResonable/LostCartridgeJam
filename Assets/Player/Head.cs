using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Head : MonoBehaviour {
    private float yaw, pitch;

    public void vUpdate() {
        pitch -= Input.GetAxis("Mouse Y") * Settings.MOUSE_SENSITIVITY * Time.deltaTime;
        yaw += Input.GetAxis("Mouse X") * Settings.MOUSE_SENSITIVITY * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, -90, 90);
        transform.parent.rotation = Quaternion.Euler(0, yaw, 0);
        transform.localRotation = Quaternion.Euler(pitch, 0, 0);
    }
}
