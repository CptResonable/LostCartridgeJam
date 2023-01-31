using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftHand : Hand {

    public override void Update() {
        //tIkTarget.position = Vector3.Lerp(tTarget.position, tWeaponTarget.position, arms.animationWeightInterpolator.t);
        //tIkTarget.rotation = Quaternion.Slerp(tTarget.rotation, tWeaponTarget.rotation, arms.animationWeightInterpolator.t);
        //tIkTarget.position = Vector3.Lerp(transform.position, tWeaponTarget.position, arms.animationWeightInterpolator.t);
        //tIkTarget.rotation = Quaternion.Slerp(transform.rotation, tWeaponTarget.rotation, arms.animationWeightInterpolator.t);
        tIkTarget.position = Vector3.Lerp(tWeaponTarget.position, transform.position, arms.animationWeightInterpolator.t);
        tIkTarget.rotation = Quaternion.Slerp(tWeaponTarget.rotation, transform.rotation, arms.animationWeightInterpolator.t);
    }

    public override void ManualFixedUpdate() {
        Debug.Log(arms.animationWeightInterpolator.t);
        tTarget.position = Vector3.Lerp(tWeaponTarget.position, character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandL).position, arms.animationWeightInterpolator.t);
        tTarget.rotation = Quaternion.Slerp(tWeaponTarget.rotation, character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandL).rotation, arms.animationWeightInterpolator.t);

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
}
