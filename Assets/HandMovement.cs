using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMovement : MonoBehaviour {
    [SerializeField] private KinematicMeasures kmTarget;
    [SerializeField] private KinematicMeasures kmThis;
    [SerializeField] private float errorAdjustmentCoef;
    [SerializeField] private float velocityChangeCoef;
    public Vector3 velocity;
    public Vector3 targetVelocity;

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    public Vector3 angularVelocity;
    public Vector3 targetAngularVelocity;

    private Rigidbody rb;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    public void DoUpdate() {
        kmTarget.DoUpdate();

        Vector3 deltaPosition = VectorUtils.FromToVector(lastPosition, transform.position);
        velocity = deltaPosition / Time.fixedDeltaTime;
        lastPosition = transform.position;

        Vector3 targetDeltaPos = VectorUtils.FromToVector(transform.position, kmTarget.transform.position);

        targetVelocity = kmTarget.velocity + targetDeltaPos * errorAdjustmentCoef;
        velocity = Vector3.Lerp(velocity, targetVelocity, Time.fixedDeltaTime * velocityChangeCoef);
        rb.velocity = velocity;

        Vector3 targetDeltaRot = -(kmTarget.transform.rotation * Quaternion.Inverse(transform.rotation)).eulerAngles;

        var deltaRot = kmTarget.transform.rotation * Quaternion.Inverse(transform.rotation);
        var eulerRot = new Vector3(Mathf.DeltaAngle(0, deltaRot.eulerAngles.x), Mathf.DeltaAngle(0, deltaRot.eulerAngles.y), Mathf.DeltaAngle(0, deltaRot.eulerAngles.z));
        Vector3 deltaRotation = eulerRot * Mathf.Deg2Rad;

        targetAngularVelocity = kmTarget.angularVelocity + deltaRotation * 10f;
        angularVelocity = Vector3.Lerp(angularVelocity, targetAngularVelocity, Time.fixedDeltaTime * velocityChangeCoef);
        rb.angularVelocity = angularVelocity;

    }
}
