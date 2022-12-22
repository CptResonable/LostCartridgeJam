using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMovement : MonoBehaviour {
    [SerializeField] private KinematicMeasures kmTarget;
    [SerializeField] private KinematicMeasures kmThis;
    [SerializeField] private float errorAdjustmentCoef;
    [SerializeField] private float velocityChangeCoef;
    [SerializeField] private Transform tCOM;

    public Vector3 velocity;
    public Vector3 targetVelocity;

    public Vector3 angularVelocity;
    public Vector3 targetAngularVelocity;

    public Vector3 rotationOffset;

    public Vector3 targetDeltaPos;

    private Rigidbody rb;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        Debug.Log(rb.inertiaTensor);
        //rb.inertiaTensor = Vector3.one;
        rb.centerOfMass = transform.InverseTransformPoint(tCOM.position);
        //rb.centerOfMass = Vector3.zero;
    }

    public void SetCOM() {
        rb = GetComponent<Rigidbody>();
        Debug.Log(rb.inertiaTensor);
        //rb.inertiaTensor = Vector3.one;
        rb.centerOfMass = transform.InverseTransformPoint(tCOM.position);
        //rb.centerOfMass = Vector3.zero;
    }

    public void DoUpdate() {
        rb = GetComponent<Rigidbody>();
        kmTarget.DoUpdate();

        velocity = rb.velocity;
        targetDeltaPos = VectorUtils.FromToVector(transform.position, kmTarget.transform.position);

        targetVelocity = kmTarget.velocity + targetDeltaPos * errorAdjustmentCoef;
        velocity = Vector3.Lerp(velocity, targetVelocity, Time.fixedDeltaTime * velocityChangeCoef);
        rb.velocity = velocity;
    }
}
