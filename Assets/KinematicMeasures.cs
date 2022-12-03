using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicMeasures : MonoBehaviour {
    public Vector3 velocity;
    public Vector3 angularVelocity;

    private Vector3 lastPosition;

    private void Awake() {
        lastPosition = transform.position;
    }

    public void DoUpdate() {
        Vector3 deltaPosition = VectorUtils.FromToVector(lastPosition, transform.position);
        velocity = deltaPosition / Time.fixedDeltaTime;
        lastPosition = transform.position;
    }
}
