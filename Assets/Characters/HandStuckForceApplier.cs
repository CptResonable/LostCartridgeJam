using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandStuckForceApplier : MonoBehaviour {

    [SerializeField] private Transform tMeshHand;
    [SerializeField] private Transform tPhysicalHand;
    [SerializeField] private Rigidbody rbCharacter;
    [SerializeField] private Rigidbody rbHand;
    [SerializeField] private float handVelocityCorrectionSpeed;
    [SerializeField] private float bodyVelocityCorrectionSpeed;

    private Vector3 delta;

    private void Update() {

    }

    private void FixedUpdate() {
        //rbHand.velocity -= delta * Time.fixedDeltaTime * handVelocityCorrectionSpeed;

        delta = VectorUtils.FromToVector(tMeshHand, tPhysicalHand);
        GizmoManager.i.DrawSphere(Time.deltaTime, Color.green, tMeshHand.position, 0.05f);
        GizmoManager.i.DrawSphere(Time.deltaTime, Color.red, tPhysicalHand.position, 0.05f);

        if (delta.magnitude > 0.07) {
            Debug.Log("dm: " + delta.magnitude);
            rbCharacter.velocity += delta * bodyVelocityCorrectionSpeed;
        }

        //if (delta.magnitude > 0.03f) {
        //    Vector3 bv = Vector3.Project(rbCharacter.velocity, delta.normalized);
        //    Vector3 hv = Vector3.Project(rbHand.velocity, delta.normalized);

        //    if (hv.magnitude < bv.magnitude) {
        //        rbHand.velocity -= hv;
        //        rbHand.velocity += bv;
        //        rbHand.velocity -= delta * Time.fixedDeltaTime * handVelocityCorrectionSpeed;
        //    }
        //    rbHand.position = tMeshHand.position;
        //}

        //rbHand.Move(tMeshHand.position, tMeshHand.rotation);
        //rbHand.AddForce(-delta * handVelocityCorrectionSpeed, ForceMode.VelocityChange);
    }

    //private void FixedUpdate() {
    //    //rbHand.velocity -= delta * Time.fixedDeltaTime * handVelocityCorrectionSpeed;
    //    Debug.Log("dm: " + delta.magnitude);

    //    //if (delta.magnitude > 0.03f) {
    //    //    Vector3 bv = Vector3.Project(rbCharacter.velocity, delta.normalized);
    //    //    Vector3 hv = Vector3.Project(rbHand.velocity, delta.normalized);

    //    //    if (hv.magnitude < bv.magnitude) {
    //    //        rbHand.velocity -= hv;
    //    //        rbHand.velocity += bv;
    //    //        rbHand.velocity -= delta * Time.fixedDeltaTime * handVelocityCorrectionSpeed;
    //    //    }
    //    //    rbHand.position = tMeshHand.position;
    //    //}

    //    //rbHand.Move(tMeshHand.position, tMeshHand.rotation);
    //    //rbHand.AddForce(-delta * handVelocityCorrectionSpeed, ForceMode.VelocityChange);
    //}
}
