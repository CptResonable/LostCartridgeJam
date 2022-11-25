using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpperBodyConnector : MonoBehaviour {
    [SerializeField] private Transform tTarget;
    private Rigidbody rb;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void Update() {
        rb.MovePosition(tTarget.position);
        rb.MoveRotation(tTarget.rotation);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}