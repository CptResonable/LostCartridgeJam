using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    private void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        Screen.SetResolution(320, 240, true);
    }
    private void Start() {
        Screen.SetResolution(320, 240, true);
    }
}
