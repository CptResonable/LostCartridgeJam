using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicMeasures : MonoBehaviour {
    public Vector3 velocity;
    public Vector3 angularVelocity;

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    private void Awake() {
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    public void DoUpdate() {

        // Velocity
        Vector3 deltaPosition = VectorUtils.FromToVector(lastPosition, transform.position);
        velocity = deltaPosition / Time.fixedDeltaTime;
        lastPosition = transform.position;

        // Angular velocity 
        var deltaRot = transform.rotation * Quaternion.Inverse(lastRotation);
        var eulerRot = new Vector3(Mathf.DeltaAngle(0, deltaRot.eulerAngles.x), Mathf.DeltaAngle(0, deltaRot.eulerAngles.y), Mathf.DeltaAngle(0, deltaRot.eulerAngles.z));
        angularVelocity = eulerRot / Time.fixedDeltaTime * Mathf.Deg2Rad;
        lastRotation = transform.rotation;
    }
}
