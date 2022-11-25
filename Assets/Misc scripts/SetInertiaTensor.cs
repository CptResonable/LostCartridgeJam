using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetInertiaTensor : MonoBehaviour {
    public Vector3 inertiaTensor = Vector3.one;
    public bool execute;

    private void Awake() {
        GetComponent<Rigidbody>().inertiaTensor = inertiaTensor;
    }

    private void Update() {
        if (execute) {
            execute = false;
            GetComponent<Rigidbody>().inertiaTensor = inertiaTensor;
        }
    }
}
