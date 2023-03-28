using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftHand : Hand {
    public enum LeftHandState { weaponGrip, grabingMag}
    [SerializeField] private Transform tMagGrabAnimationPoint;

    public override void Init(Character character) {
        base.Init(character);

        character.animatorController.animatorUpdatedEvent += AnimatorController_animatorUpdatedEvent;
    }

    public override void Update() {
        base.Update();

        if (character.equipmentManager.equipedItem != null) {
            tWeaponTarget.position = character.equipmentManager.equipedItem.tOffHandTarget.position;
            tWeaponTarget.rotation = character.equipmentManager.equipedItem.tOffHandTarget.rotation;
        }

        // Interpolate between physical hand and weapon grip, reason is i don't want grip hand to wobble
        tIkTarget.position = Vector3.Lerp(tWeaponTarget.position, transform.position, arms.animationWeight);
        tIkTarget.rotation = Quaternion.Slerp(tWeaponTarget.rotation, transform.rotation, arms.animationWeight);

        // Interpolate between hand target and mag belt while reloading
        if (character.weaponController.state == WeaponController.State.weaponEquiped) {
            Gun gun = (Gun)character.weaponController.equipedItem;
            tIkTarget.position = Vector3.Lerp(tIkTarget.position, tMagGrabAnimationPoint.position, gun.gunAnimationController.leftHandMagGrabT);
            tIkTarget.rotation = Quaternion.Lerp(tIkTarget.rotation, tMagGrabAnimationPoint.rotation, gun.gunAnimationController.leftHandMagGrabT);
        }
    }

    protected override void Character_fixedUpdateEvent() {
        base.Character_fixedUpdateEvent();

        // Interpolate between weapon hand target and animation position/rotation
        tPhysicalTarget.position = Vector3.Lerp(tWeaponTarget.position, character.body.postAnimationState.GetBoneState(Body.BoneEnums.HandL).position, arms.animationWeight);
        tPhysicalTarget.rotation = Quaternion.Slerp(tWeaponTarget.rotation, character.body.postAnimationState.GetBoneState(Body.BoneEnums.HandL).rotation, arms.animationWeight);

        // Set postion/rotation to ledge grab point if grabing ledge
        tPhysicalTarget.position = Vector3.Lerp(tPhysicalTarget.position, grabPoint, ledgeGrabInterpolator);
        tPhysicalTarget.rotation = Quaternion.Slerp(tPhysicalTarget.rotation, grabRotation, ledgeGrabInterpolator);

        WallAvoidance();

        if (character.locomotion.activeStateEnum == Locomotion.LocomotionState.StateIDEnum.WallClimbing && !grabingLedge)
            LookForGrip();

        PhysicalHandUpdate();
    }


    private void AnimatorController_animatorUpdatedEvent() {
        tElbowPole.transform.position = Vector3.Lerp(tElbowNoAnimPoleTarget.position, character.body.postAnimationState.GetBoneState(Body.BoneEnums.ArmL_2).position, arms.animationWeight);
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

        if (Physics.Raycast(tPhysicalTarget.position + wallHit.normal, -wallHit.normal, out hit, 1.2f, LayerMasks.i.environment)) {
            tPhysicalTarget.position = hit.point;
        }
    }

    //private bool LookForGrip() {

    //    if (character.locomotion.wallrunController.ledgeGrabPoints.Count == 0)
    //        return false;

    //    if (tPhysicalTarget.position.y > character.locomotion.wallrunController.topGrabPoint.y) {

    //        Vector3 v3 = Vector3.ProjectOnPlane(VectorUtils.FromToVector(character.locomotion.wallrunController.topGrabPoint, tPhysicalTarget.position), character.transform.forward);
    //        Vector3 point = character.locomotion.wallrunController.topGrabPoint + new Vector3(v3.x, 0, v3.z);

    //        RaycastHit hit;
    //        if (Physics.Raycast(point + Vector3.up * 0.1f, Vector3.down, out hit, 0.2f, LayerMasks.i.environment)) {
    //            grabPoint = hit.point;
    //            grabRotation = Quaternion.LookRotation(-Vector3.Cross(character.locomotion.wallrunController.wallHit.normal, hit.normal), -character.locomotion.wallrunController.wallHit.normal);
    //        }
    //        else {
    //            grabPoint = character.locomotion.wallrunController.topGrabPoint;
    //            grabRotation = Quaternion.LookRotation(-Vector3.Cross(character.locomotion.wallrunController.wallHit.normal, Vector3.up), -character.locomotion.wallrunController.wallHit.normal);
    //        }
    //        grabingLedge = true;

    //        return true;
    //    }
    //    else {
    //        return false;
    //    }
    //}

    //private bool LookForGrip() {
    //    Vector3 direction = -character.locomotion.wallrunController.wallHit.normal;

    //    RaycastHit hit1;
    //    if (Physics.Raycast(tPhysicalTarget.position - Vector3.up * 0.025f - direction * 0.5f, direction, out hit1, 1f, LayerMasks.i.environment)) {
    //        RaycastHit hit2;
    //        if (Physics.Raycast(hit1.point + direction * 0.05f + Vector3.up * 0.1f, Vector3.down, out hit2, 0.2f, LayerMasks.i.environment)) {
    //            if (Vector3.Angle(hit2.normal, Vector3.up) < 15f && Vector3.Angle(hit2.normal, direction) > 75f) {
    //                grabingLedge = true;
    //                grabPoint = hit2.point + Vector3.up * 0.005f;
    //                grabRotation = Quaternion.LookRotation(Vector3.Cross(hit1.normal, hit2.normal), -hit1.normal);
    //                return true;
    //            }
    //        }

    //        return false;
    //    }
    //    else
    //        return false;
    //}
}
