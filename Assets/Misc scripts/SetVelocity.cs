using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVelocity : MonoBehaviour {
    [SerializeField] private Vector3 velocity;
    private Rigidbody rb;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void LateUpdate() {
        rb.velocity = velocity;
    }
}
