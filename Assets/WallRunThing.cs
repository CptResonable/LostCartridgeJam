using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunThing : MonoBehaviour {
    [SerializeField] private Rigidbody targetRb;
    [SerializeField] private float scale;
    [SerializeField] private float resetSpeed;

    private Vector3 offset;
    private Vector3 lastRbVelocity;
    private Vector3 rbAcc;

    private void FixedUpdate() {
        rbAcc = VectorUtils.FromToVector(lastRbVelocity, targetRb.velocity) / Time.fixedDeltaTime;
        lastRbVelocity = targetRb.velocity;

        Vector3 flatAcc = Vector3.ProjectOnPlane(rbAcc, Vector3.up);
        float angle =  Vector3.Angle(Vector3.ProjectOnPlane(targetRb.velocity.normalized, Vector3.up), flatAcc.normalized);
        angle /= 180;
        //angle = 1 - angle;
        angle = Mathf.Pow(angle, 4);

        offset -= rbAcc * angle * scale;

        offset = Vector3.Lerp(offset, Vector3.zero, Time.deltaTime * resetSpeed);
        transform.position = targetRb.transform.position + offset;
    }
}
