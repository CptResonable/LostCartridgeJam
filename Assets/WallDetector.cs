using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDetector : MonoBehaviour {
    public bool wallDetected = false;

    private Collider collider;

    private int triggerCount = 0;

    private void Awake() {
        collider = GetComponent<Collider>();
    }

    //public event Delegates.EmptyDelegate
    private void OnTriggerEnter(Collider other) {
        wallDetected = true;
        triggerCount++;
    }

    private void OnTriggerExit(Collider other) {
        triggerCount--;
        if (triggerCount == 0)
            wallDetected = false;
    }
    private void OnTriggerStay(Collider other) {
        Debug.Log("trigger: " + other.name);
    }
}
