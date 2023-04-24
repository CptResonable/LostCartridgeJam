using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour {
    private void Update() {
        if (GameManager.i.player == null)
            return;

        transform.LookAt(GameManager.i.player.head.tCameraBase.transform);
    }
}
