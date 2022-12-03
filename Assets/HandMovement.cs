using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMovement : MonoBehaviour {
    [SerializeField] private KinematicMeasures kmTarget;
    [SerializeField] private float errorAdjustmentCoef;
    [SerializeField] private float velocityChangeCoef;
    public Vector3 velocity;
    public Vector3 targetVelocity;

    private Vector3 lastPosition;
    public void DoUpdate() {
        kmTarget.DoUpdate();

        Vector3 deltaPosition = VectorUtils.FromToVector(lastPosition, transform.position);
        velocity = deltaPosition / Time.fixedDeltaTime;
        lastPosition = transform.position;

        Vector3 targetDeltaPos = VectorUtils.FromToVector(transform.position, kmTarget.transform.position);

        targetVelocity = kmTarget.velocity + targetDeltaPos * errorAdjustmentCoef;
        velocity = Vector3.Lerp(velocity, targetVelocity, Time.fixedDeltaTime * velocityChangeCoef);
        GetComponent<Rigidbody>().velocity = velocity;
    }
}
