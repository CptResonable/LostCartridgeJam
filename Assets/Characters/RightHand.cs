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
        tPhysicalTarget.position = Vector3.Lerp(tWeaponTarget.position, character.body.postAnimationState.GetBoneState(Body.BoneEnums.HandR).position, arms.animationWeight);
        tPhysicalTarget.rotation = Quaternion.Slerp(tWeaponTarget.rotation, character.body.postAnimationState.GetBoneState(Body.BoneEnums.HandR).rotation, arms.animationWeight);

        // Set postion/rotation to ledge grab point if grabing ledge
        tPhysicalTarget.position = Vector3.Lerp(tPhysicalTarget.position, grabPoint, ledgeGrabInterpolator);
        tPhysicalTarget.rotation = Quaternion.Slerp(tPhysicalTarget.rotation, grabRotation, ledgeGrabInterpolator);

        Vector3 shoulderToHandTarget = VectorUtils.FromToVector(character.body.tArmR_1.position, tPhysicalTarget.position);
        if (shoulderToHandTarget.magnitude > 0.55f)
            tPhysicalTarget.position = character.body.tArmR_1.position + shoulderToHandTarget.normalized * 0.55f;

        //// Use player velocity to do some prediction (Otherwise hand will drift away from player while falling)
        //if (tPhysicalTarget.position.y > character.body.tTorso_2.position.y)
        //    tPhysicalTarget.position += character.rb.velocity * 0.5f * Time.fixedDeltaTime;

        WallAvoidance();

        if (character.locomotion.wallrunController.isWallRunning && !grabingLedge)
            LookForGrip();

        PhysicalHandUpdate();
    }

    private void AnimatorController_animatorUpdatedEvent() {
        float f = arms.animationWeight;
        if (character.locomotion.wallrunController.isWallRunning)
            f *= 0.4f;
        tElbowPole.transform.position = Vector3.Lerp(tElbowNoAnimPoleTarget.position, character.body.postAnimationState.GetBoneState(Body.BoneEnums.ArmR_2).position, f);
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
    private void WallAvoidance() {
        if (character.locomotion.wallrunController.isWallRunning) {
            RaycastHit wallHit = character.locomotion.wallrunController.wallHit;
            RaycastHit hit;

            if (Physics.Raycast(tPhysicalTarget.position + wallHit.normal, -wallHit.normal, out hit, 1.2f, LayerMasks.i.environment)) {
                tPhysicalTarget.position = hit.point;
            }
        }
    }
}
