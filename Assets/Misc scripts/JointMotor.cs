using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointMotor : MonoBehaviour {
    public Quaternion deltaRot;
    [SerializeField] private Transform target;
    [SerializeField] private bool configuredInWorldSpace;

    private ConfigurableJoint joint;
    private Quaternion startLocalRot;
    private Quaternion startWorldRot;

    private void Awake() {
        joint = GetComponent<ConfigurableJoint>();
        startLocalRot = transform.localRotation;
        startWorldRot = transform.rotation;
    }

    private void FixedUpdate() {
        Quaternion t = target.rotation;

        if (configuredInWorldSpace)
            joint.SetTargetRotation(t, startWorldRot);
        else
            joint.SetTargetRotationLocal(Quaternion.identity, startLocalRot);
        //joint.SetTargetRotationLocal(target.localRotation, startLocalRot);
    }
}
