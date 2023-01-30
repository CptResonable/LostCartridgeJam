using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMovement : MonoBehaviour {
    [SerializeField] private KinematicMeasures kmTarget;
    [SerializeField] private float errorAdjustmentCoef;
    [SerializeField] private float velocityChangeCoef;
    [SerializeField] private Transform tCOM;
    [SerializeField] private Character character;
    [SerializeField] private Transform tIkTarget;

    public Enums.Side side;

    public Vector3 velocity;
    public Vector3 targetVelocity;

    public Vector3 angularVelocity;
    public Vector3 targetAngularVelocity;

    public Vector3 rotationOffset;

    public Vector3 targetDeltaPos;

    private Rigidbody rb;
    private ConfigurableJoint joint;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        SetCOM();
    }

    private void Update() {
        tIkTarget.position = transform.position;
        tIkTarget.rotation = transform.rotation;
    }

    public void SetCOM() {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = transform.InverseTransformPoint(tCOM.position);
    }

    public void SetInertiaTensor(Vector3 inertiaTensor) {
        rb.inertiaTensor = inertiaTensor;
    }


    public void DoUpdate() {
        if (side == Enums.Side.right)
            DoUpdateRight();
        else
            DoUpdateLeft();
    }

    private void DoUpdateRight() {
        kmTarget.DoUpdate();

        velocity = rb.velocity;
        targetDeltaPos = VectorUtils.FromToVector(transform.position, kmTarget.transform.position);

        targetVelocity = kmTarget.velocity + targetDeltaPos * errorAdjustmentCoef;
        velocity = Vector3.Lerp(velocity, targetVelocity, Time.fixedDeltaTime * velocityChangeCoef);
        rb.velocity = velocity;

        //tIkTarget.position = transform.position;
        //tIkTarget.rotation = transform.rotation;
        Debug.Log(name + " it " + rb.inertiaTensor);
        Debug.Log(name + " com " + rb.centerOfMass);
    }

    private void DoUpdateLeft() {
        kmTarget.DoUpdate();

        if (character.playerController.isSprining) {
            //joint.angularXMotion = ConfigurableJointMotion.Locked;
            //joint.angularYMotion = ConfigurableJointMotion.Locked;
            //joint.angularZMotion = ConfigurableJointMotion.Locked;

            ////rb.isKinematic = true;
            //transform.position = kmTarget.transform.position;
            //transform.rotation = kmTarget.transform.rotation;

            velocity = rb.velocity;
            targetDeltaPos = VectorUtils.FromToVector(transform.position, kmTarget.transform.position);

            targetVelocity = kmTarget.velocity + targetDeltaPos * errorAdjustmentCoef;
            velocity = Vector3.Lerp(velocity, targetVelocity, Time.fixedDeltaTime * velocityChangeCoef);
            rb.velocity = velocity;
        }
        else {
            //rb.isKinematic = false;

            //joint.angularXMotion = ConfigurableJointMotion.Limited;
            //joint.angularYMotion = ConfigurableJointMotion.Limited;
            //joint.angularZMotion = ConfigurableJointMotion.Limited;
            velocity = rb.velocity;
            targetDeltaPos = VectorUtils.FromToVector(transform.position, kmTarget.transform.position);

            targetVelocity = kmTarget.velocity + targetDeltaPos * errorAdjustmentCoef;
            velocity = Vector3.Lerp(velocity, targetVelocity, Time.fixedDeltaTime * velocityChangeCoef);
            rb.velocity = velocity;

            //tIkTarget.position = kmTarget.transform.position;
            //tIkTarget.rotation = kmTarget.transform.rotation;
        }

        Debug.Log(name + " it " + rb.inertiaTensor);
        Debug.Log(name + " com " + rb.centerOfMass);

        //velocity = rb.velocity;
        //targetDeltaPos = VectorUtils.FromToVector(transform.position, kmTarget.transform.position);

        //targetVelocity = kmTarget.velocity + targetDeltaPos * errorAdjustmentCoef;
        //velocity = Vector3.Lerp(velocity, targetVelocity, Time.fixedDeltaTime * velocityChangeCoef);
        //rb.velocity = velocity;

    }
}
