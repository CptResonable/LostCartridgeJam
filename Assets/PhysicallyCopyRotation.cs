using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VacuumBreather;

public class PhysicallyCopyRotation : MonoBehaviour {
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 PidValues;
    [SerializeField] private Mode mode;

    public float strengthMod = 1;
    private float lastError = 0; // Used to calculate delta error, only used in y and xz mode

    private enum Mode { full, yAxis, xzAxis}

    private Rigidbody rb;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {

        if (mode == Mode.full)
            Full();
        else if (mode == Mode.yAxis)
            YAxis();
        else if (mode == Mode.xzAxis)
            XZAxis();
    }

    private void Full() {
        PidQuaternionController pidController = new PidQuaternionController(PidValues.x, PidValues.y, PidValues.z);
        Vector3 output = pidController.ComputeRequiredAngularAcceleration(transform.rotation, target.rotation, rb.angularVelocity, Time.deltaTime);
        rb.AddTorque(output * strengthMod, ForceMode.Acceleration);
        //rb.AddTorque(output * 0.2f, ForceMode.Force);
    }

    private void YAxis() {
        PidController pidController = new PidController(PidValues.x, PidValues.y, PidValues.z);

        Vector3 f = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        Vector3 tf = Vector3.ProjectOnPlane(target.forward, Vector3.up);
        float error = Vector3.SignedAngle(f, tf, Vector3.up);
        float dError = error - lastError;
        float output = pidController.ComputeOutput(error, dError, Time.deltaTime);

        rb.AddTorque(Vector3.up * strengthMod * output);

        lastError = error;
    }

    private void XZAxis() {
        PidController pidController = new PidController(PidValues.x, PidValues.y, PidValues.z);
        float error = Vector3.Angle(transform.up, target.up);
        float dError = error - lastError;
        float output = pidController.ComputeOutput(error, dError, Time.deltaTime);

        rb.AddTorque(Vector3.Cross(transform.up, target.up) * output * strengthMod);

        lastError = error;
    }
}
