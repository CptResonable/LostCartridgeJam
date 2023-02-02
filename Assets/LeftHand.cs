using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftHand : Hand {

    private RaycastHit hit;
    [SerializeField] private LayerMask layerMask;

    public override void Update() {

        // Interpolate between physical hand and weapon grip, reason is i don't want grip hand to wobble
        tIkTarget.position = Vector3.Lerp(tWeaponTarget.position, transform.position, arms.animationWeightInterpolator.t);
        tIkTarget.rotation = Quaternion.Slerp(tWeaponTarget.rotation, transform.rotation, arms.animationWeightInterpolator.t);
    }

    public override void ManualFixedUpdate() {

        // Interpolate physics target pos/rot between weapon grip and animation
        tPhysicalTarget.position = Vector3.Lerp(tWeaponTarget.position, character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandL).position, arms.animationWeightInterpolator.t);
        tPhysicalTarget.rotation = Quaternion.Slerp(tWeaponTarget.rotation, character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandL).rotation, arms.animationWeightInterpolator.t);

        WallAvoidance();

        PhysicalHandUpdate();
    }

    private void PhysicalHandUpdate() {
        kmTarget.DoUpdate();

        velocity = rb.velocity;
        targetDeltaPos = VectorUtils.FromToVector(transform.position, kmTarget.transform.position);

        targetVelocity = kmTarget.velocity + targetDeltaPos * errorAdjustmentCoef;
        velocity = Vector3.Lerp(velocity, targetVelocity, Time.fixedDeltaTime * velocityChangeCoef);
        rb.velocity = velocity;
    }

    private void WallAvoidance() {
        RaycastHit wallHit = character.locomotion.wallrunController.wallHit;

        // Raycast down from foot
        if (Physics.Raycast(tPhysicalTarget.position + wallHit.normal, -wallHit.normal, out hit, 4, layerMask)) {
            tPhysicalTarget.position = hit.point;
        }
    }

    //private void WallClimbUpdate() {
    //    RaycastHit wallHit = character.locomotion.wallrunController.wallHit;

    //    // Raycast down from foot
    //    if (Physics.Raycast(tHand.position + wallHit.normal, -wallHit.normal, out hit, 4, layerMask)) {
    //        transform.position = hit.point;
    //    }

    //    transform.rotation = tHand.rotation;

    //    // Looking left or right
    //    if (character.locomotion.wallrunController.wallCameraAngle < -10 && side == Enums.Side.right || character.locomotion.wallrunController.wallCameraAngle > 10 && side == Enums.Side.left) {

    //        Vector3 sideLookHandPosition = character.head.tCamera.position + character.head.tCamera.TransformVector(handCameraPosition);
    //        Quaternion sideLookHandRotation = character.head.tCamera.rotation * Quaternion.Euler(handCameraRotation);

    //        float t = wallAngleToHandLookCurve.Evaluate(Mathf.Abs(character.locomotion.wallrunController.wallCameraAngle));
    //        transform.position = Vector3.Lerp(transform.position, sideLookHandPosition, t);
    //        transform.rotation = Quaternion.Slerp(transform.rotation, sideLookHandRotation, t);
    //    }

    //    if (!grabingLedge) {
    //        if (Input.GetKey(KeyCode.Mouse1)) {
    //            if (LookForGrip())
    //                GrabLedge();
    //        }
    //    }
    //}
}
