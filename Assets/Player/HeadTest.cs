using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadTest : MonoBehaviour {
    public float yaw, pitch;
    [SerializeField] private Transform t1;
    [SerializeField] private Transform t2;
    [SerializeField] private HandMovement hm;

    private void Update() {
        pitch -= Input.GetAxis("Mouse Y") * Settings.MOUSE_SENSITIVITY;
        yaw += Input.GetAxis("Mouse X") * Settings.MOUSE_SENSITIVITY;

        pitch = Mathf.Clamp(pitch, -89, 89);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0);

        t1.rotation = Quaternion.Slerp(t1.rotation, transform.rotation, Time.deltaTime * 40);
        t2.rotation = transform.rotation;
    }

    private void FixedUpdate() {
        hm.DoUpdate();
    }
}
