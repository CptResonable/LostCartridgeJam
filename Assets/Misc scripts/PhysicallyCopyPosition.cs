using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VacuumBreather;

public class PhysicallyCopyPosition : MonoBehaviour {
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 pidValues;

    private Rigidbody rb;
    private Vector3 error = Vector3.zero;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    public void FixedUpdate() {
        Vector3 newError = VectorUtils.FromToVector(transform.position, target.position);
        Vector3 dError = VectorUtils.FromToVector(error, newError);
        error = newError;

        PidController pidX = new PidController(pidValues.x, pidValues.y, pidValues.z);
        PidController pidY = new PidController(pidValues.x, pidValues.y, pidValues.z);
        PidController pidZ = new PidController(pidValues.x, pidValues.y, pidValues.z);

        Vector3 output;
        output.x = pidX.ComputeOutput(error.x, dError.x, Time.deltaTime);
        output.y = pidY.ComputeOutput(error.y, dError.y, Time.deltaTime);
        output.z = pidZ.ComputeOutput(error.z, dError.z, Time.deltaTime);

        rb.AddForce(output * 20 * Time.deltaTime);
    }
}
