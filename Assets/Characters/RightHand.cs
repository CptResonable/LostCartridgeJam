using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightHand : Hand {

    [SerializeField] private Transform tRightHandAnimator;
    private Animator rightHandAnimator;

    private Vector3 handRotationOffset;


    public override void Init(Character character) {
        base.Init(character);

        rightHandAnimator = tRightHandAnimator.GetComponent<Animator>();

        character.animatorController.animatorUpdatedEvent += AnimatorController_animatorUpdatedEvent;
    }

    public override void Update() {
        base.Update();

        // Set ik target to physical hand
        tIkTarget.position = transform.position;
        tIkTarget.rotation = transform.rotation;
    }


    protected override void Character_fixedUpdateEvent() {
        base.Character_fixedUpdateEvent();

        WeaponTargetUpdate();

        // Interpolate between weapon hand target and animation position/rotation
        tPhysicalTarget.position = Vector3.Lerp(tWeaponTarget.position, character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandR).position, arms.animationWeight);
        tPhysicalTarget.rotation = Quaternion.Slerp(tWeaponTarget.rotation, character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandR).rotation, arms.animationWeight);

        // Set postion/rotation to ledge grab point if grabing ledge
        tPhysicalTarget.position = Vector3.Lerp(tPhysicalTarget.position, grabPoint, ledgeGrabInterpolator);
        tPhysicalTarget.rotation = Quaternion.Slerp(tPhysicalTarget.rotation, grabRotation, ledgeGrabInterpolator);

        if (character.locomotion.wallrunController.isWallRunning)
            LookForGrip();

        PhysicalHandUpdate();
    }

    private void AnimatorController_animatorUpdatedEvent() {
        float f = arms.animationWeight;
        if (character.locomotion.wallrunController.isWallRunning)
            f *= 0.4f;
        tElbowPole.transform.position = Vector3.Lerp(tElbowNoAnimPoleTarget.position, character.body.postAnimationState.GetBoneState(Body.BoneEnums.rArmR_2).position, f);
    }

    private void WeaponTargetUpdate() {
        tRightHandAnimator.Rotate(character.weaponController.equipedGun.tRightHandOffset.localRotation.eulerAngles, Space.Self);
        tRightHandAnimator.localPosition = character.weaponController.equipedGun.tRightHandOffset.localPosition;

        handRotationOffset = new Vector3(handRotationOffset.x, handRotationOffset.y, Mathf.Lerp(handRotationOffset.z, -character.characterInput.moveInput.x * 25 + character.rb.angularVelocity.y * -2, Time.fixedDeltaTime * 8));

        if (character.weaponController.equipedGun != null)
            tWeaponTarget.localPosition = Vector3.Lerp(character.weaponController.equipedGun.targetHandPosition, character.weaponController.equipedGun.targetAdsHandPosition, arms.hipAdsInterpolator.t);

        tWeaponTarget.rotation = character.fpCamera.tCamera.rotation;
        tWeaponTarget.Rotate(handRotationOffset);
        tWeaponTarget.Rotate(new Vector3(-90, 0, 180));

    }

    private void PhysicalHandUpdate() {
        kmTarget.DoUpdate();

        velocity = rb.velocity;

        targetDeltaPos = VectorUtils.FromToVector(transform.position, kmTarget.transform.position);

        targetVelocity = kmTarget.velocity + targetDeltaPos * errorAdjustmentCoef;
        velocity = Vector3.Lerp(velocity, targetVelocity, Time.fixedDeltaTime * velocityChangeCoef);
        rb.velocity = velocity;
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
                    grabRotation = Quaternion.LookRotation(-Vector3.Cross(hit1.normal, hit2.normal), -hit1.normal);
                    return true;
                }
            }

            return false;
        }
        else
            return false;
    }
}
