using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftHand : Hand {
    public override void Init(Character character) {
        base.Init(character);

        character.animatorController.animatorUpdatedEvent += AnimatorController_animatorUpdatedEvent;
    }

    public override void Update() {

        // Interpolate between physical hand and weapon grip, reason is i don't want grip hand to wobble
        tIkTarget.position = Vector3.Lerp(tWeaponTarget.position, transform.position, arms.animationWeight);
        tIkTarget.rotation = Quaternion.Slerp(tWeaponTarget.rotation, transform.rotation, arms.animationWeight);

        if (arms.animationWeight < 0.01f)
            grabingLedge = false;
    }

    public override void ManualFixedUpdate() {

        Vector3 wallRunPosition = character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandL).position;
        Quaternion wallRunRotation = character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandL).rotation;

        if (grabingLedge) {
            wallRunPosition = grabPoint;
            wallRunRotation = grabRotation;
        }

        tPhysicalTarget.position = Vector3.Lerp(tWeaponTarget.position, wallRunPosition, arms.animationWeight);
        tPhysicalTarget.rotation = Quaternion.Slerp(tWeaponTarget.rotation, wallRunRotation, arms.animationWeight);

        WallAvoidance();

        if (character.locomotion.wallrunController.isWallRunning)
            LookForGrip();

        PhysicalHandUpdate();
    }


    private void AnimatorController_animatorUpdatedEvent() {
        tElbowPole.transform.position = Vector3.Lerp(tElbowNoAnimPoleTarget.position, character.body.postAnimationState.GetBoneState(Body.BoneEnums.rArmL_2).position, arms.animationWeight);
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
        RaycastHit hit;

        if (Physics.Raycast(tPhysicalTarget.position + wallHit.normal, -wallHit.normal, out hit, 0.4f, LayerMasks.i.environment)) {
            tPhysicalTarget.position = hit.point;
        }
    }

    private bool LookForGrip() {
        Vector3 direction = -character.locomotion.wallrunController.wallHit.normal;

        RaycastHit hit1;
        if (Physics.Raycast(tPhysicalTarget.position - Vector3.up * 0.025f - direction * 0.5f, direction, out hit1, 1f, LayerMasks.i.environment)) {
            RaycastHit hit2;
            if (Physics.Raycast(hit1.point + direction * 0.05f + Vector3.up * 0.1f, Vector3.down, out hit2, 0.2f, LayerMasks.i.environment)) {
                if (Vector3.Angle(hit2.normal, Vector3.up) < 15f && Vector3.Angle(hit2.normal, direction) > 75f) {
                    grabingLedge = true;
                    grabPoint = hit2.point + Vector3.up * 0.005f;
                    grabRotation = Quaternion.LookRotation(Vector3.Cross(hit1.normal, hit2.normal), -hit1.normal);
                    return true;
                }
            }

            return false;
        }
        else
            return false;
    }

    //private bool LookForGrip() {
    //    Vector3 direction = -character.locomotion.wallrunController.wallHit.normal;

    //    RaycastHit hit1;
    //    if (Physics.Raycast(tPhysicalTarget.position - Vector3.up * 0.025f - direction * 0.5f, direction, out hit1, 0.4f, LayerMasks.i.environment)) {

    //        RaycastHit hit2;
    //        if (!Physics.Raycast(tPhysicalTarget.position + Vector3.up * 0.025f - direction * 0.5f, direction, out hit2, 0.4f, LayerMasks.i.environment)) {

    //            RaycastHit hit3;
    //            if (Physics.Raycast(hit1.point + direction * 0.05f + Vector3.up * 0.1f, Vector3.down, out hit3, 0.2f, LayerMasks.i.environment)) {
    //                if (Vector3.Angle(hit3.normal, Vector3.up) < 15f && Vector3.Angle(hit3.normal, direction) > 75f) {
    //                    grabingLedge = true;
    //                    grabPoint = hit3.point + Vector3.up * 0.005f;
    //                    grabRotation = Quaternion.LookRotation(-Vector3.Cross(hit1.normal, hit3.normal), -hit1.normal);
    //                    return true;
    //                }
    //            }
    //        }

    //        return false;
    //    }
    //    else
    //        return false;
    //}
}
