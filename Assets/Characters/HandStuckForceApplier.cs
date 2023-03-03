using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandStuckForceApplier : MonoBehaviour {

    [SerializeField] private Transform tMeshHand;
    [SerializeField] private Transform tPhysicalHand;
    [SerializeField] private Rigidbody rbCharacter;
    [SerializeField] private Rigidbody rbHand;
    [SerializeField] private float handVelocityCorrectionSpeed;

    private Vector3 delta;

    private void LateUpdate() {
        delta = VectorUtils.FromToVector(tMeshHand, tPhysicalHand);
    }

    private void FixedUpdate() {
        //rbHand.velocity -= delta * Time.fixedDeltaTime * handVelocityCorrectionSpeed;

        if (delta.magnitude > 0.03f) {
            Vector3 bv = Vector3.Project(rbCharacter.velocity, delta.normalized);
            Vector3 hv = Vector3.Project(rbHand.velocity, delta.normalized);

            if (hv.magnitude < bv.magnitude) {
                rbHand.velocity -= hv;
                rbHand.velocity += bv;
                rbHand.velocity -= delta * Time.fixedDeltaTime * handVelocityCorrectionSpeed;
            }
            rbHand.position = tMeshHand.position;
        }

        //rbHand.Move(tMeshHand.position, tMeshHand.rotation);
        //rbHand.AddForce(-delta * handVelocityCorrectionSpeed, ForceMode.VelocityChange);
    }
}
