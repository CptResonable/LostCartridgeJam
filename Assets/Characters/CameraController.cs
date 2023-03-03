using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    [SerializeField] private Camera[] cameras;
    private int activeCameraIndex;

    private void Awake() {
        for (int i = 1; i < cameras.Length; i++) {
            cameras[i].enabled = false;
            Debug.Log(cameras[i].name);
        }
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.F1)) {
            cameras[activeCameraIndex].enabled = false;
            activeCameraIndex++;

            if (activeCameraIndex >= cameras.Length)
                activeCameraIndex = 0;

            cameras[activeCameraIndex].enabled = true;
        }
    }
}
