using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightHand : Hand {

    private Vector3 handRotationOffset;
    private Coroutine reloadCorutine;
    private float reloadSpinPitch;

    public override void Init(Character character) {
        base.Init(character);

        character.weaponController.reloadStartedEvent += WeaponController_reloadStartedEvent;
    }

    public override void Update() {

        // Set ik target to physical hand
        tIkTarget.position = transform.position;
        tIkTarget.rotation = transform.rotation;

        //if (!character.locomotion.wallrunController.isWallRunning)
        //    grabingLedge = false;

        //if (Input.GetKeyDown(KeyCode.Mouse4))
        //    grabingLedge = false;

        if (arms.animationWeightInterpolator.t < 0.01f)
            grabingLedge = false;

        if (Input.GetKey(KeyCode.Mouse4))
            if (LookForGrip())
                GrabLedge();
    }

    public override void ManualFixedUpdate() {
        WeaponTargetUpdate();

        // Interpolate physics target pos/rot between weapon grip and animation

        //    tPhysicalTarget.position = Vector3.Lerp(tWeaponTarget.position, WallClimbUpdate(), arms.animationWeightInterpolator.t);
        //else
        //if (character.locomotion.wallrunController.isWallRunning) {
        //    if (!LookForGrip())
        //        tPhysicalTarget.position = Vector3.Lerp(tWeaponTarget.position, character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandR).position, arms.animationWeightInterpolator.t);
        //}
        //else

        Vector3 wallRunPosition = character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandR).position;
        Quaternion wallRunRotation = character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandR).rotation;

        if (grabingLedge) {
            wallRunPosition = grabPoint;
            wallRunRotation = grabRotation;
        }

        //if (grabingLedge) {
        //    tPhysicalTarget.position = grabPoint;
        //    tPhysicalTarget.rotation = grabRotation;
        //}
        //else {
        //    tPhysicalTarget.position = Vector3.Lerp(tWeaponTarget.position, character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandR).position, arms.animationWeightInterpolator.t);
        //    tPhysicalTarget.rotation = Quaternion.Slerp(tWeaponTarget.rotation, character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandR).rotation, arms.animationWeightInterpolator.t);
        //}

        tPhysicalTarget.position = Vector3.Lerp(tWeaponTarget.position, wallRunPosition, arms.animationWeightInterpolator.t);
        tPhysicalTarget.rotation = Quaternion.Slerp(tWeaponTarget.rotation, wallRunRotation, arms.animationWeightInterpolator.t);

        if (character.locomotion.wallrunController.isWallRunning)
            LookForGrip();
        //if (character.locomotion.wallrunController.isWallRunning)
        //    Test();

        PhysicalHandUpdate();
    }

    private void WeaponTargetUpdate() {
        handRotationOffset = new Vector3(handRotationOffset.x, handRotationOffset.y, Mathf.Lerp(handRotationOffset.z, -character.characterInput.moveInput.x * 25 + character.rb.angularVelocity.y * -2, Time.fixedDeltaTime * 8));

        if (character.weaponController.equipedGun != null)
            tWeaponTarget.localPosition = Vector3.Lerp(character.weaponController.equipedGun.targetHandPosition, character.weaponController.equipedGun.targetAdsHandPosition, arms.hipAdsInterpolator.t);

        tWeaponTarget.rotation = character.fpCamera.tCamera.rotation;
        tWeaponTarget.Rotate(handRotationOffset);
        tWeaponTarget.Rotate(new Vector3(-90, 0, 180));

        if (reloadSpinPitch != 0) {
            tWeaponTarget.Rotate(new Vector3(0, 0, -50), Space.Self);
            tWeaponTarget.Rotate(new Vector3(-reloadSpinPitch, 0, 0), Space.Self);
        }
    }

    private void Test() {
        RaycastHit wallHit = character.locomotion.wallrunController.wallHit;
        Vector3 wallUp = Vector3.ProjectOnPlane(Vector3.up, wallHit.normal);
        Vector3 startPos = wallHit.point + wallUp * 2 - wallHit.normal * 0.05f;

        RaycastHit hit;
        if (Physics.Raycast(startPos, -wallUp, out hit, 2, LayerMasks.i.environment)) {
            if (tPhysicalTarget.transform.position.y > hit.point.y)
                tPhysicalTarget.transform.position = hit.point;
        }

        //RaycastHit hit;
        //if (Physics.Raycast(position, -wallHit.normal, out hit, 1, LayerMasks.i.environment)) {
        //    position = hit.point;
        //    Debug.Log("HIT");
        //}
    }

    private Vector3 WallClimbUpdate() {

        Vector3 position = new Vector3(character.transform.position.x, character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandR).position.y, character.transform.position.z);
        RaycastHit wallHit = character.locomotion.wallrunController.wallHit;

        // Raycast down from foot
        RaycastHit hit;
        if (Physics.Raycast(position, -wallHit.normal, out hit, 1, LayerMasks.i.environment)) {
            position = hit.point;
            Debug.Log("HIT");
        }
        Debug.Log("MISS");

        return position;

        //tPhysicalTarget.rotation = tHand.rotation;

        //// Looking left or right
        //if (character.locomotion.wallrunController.wallCameraAngle < -10 && side == Enums.Side.right || character.locomotion.wallrunController.wallCameraAngle > 10 && side == Enums.Side.left) {

        //    Vector3 sideLookHandPosition = character.head.tCamera.position + character.head.tCamera.TransformVector(handCameraPosition);
        //    Quaternion sideLookHandRotation = character.head.tCamera.rotation * Quaternion.Euler(handCameraRotation);

        //    float t = wallAngleToHandLookCurve.Evaluate(Mathf.Abs(character.locomotion.wallrunController.wallCameraAngle));
        //    transform.position = Vector3.Lerp(transform.position, sideLookHandPosition, t);
        //    transform.rotation = Quaternion.Slerp(transform.rotation, sideLookHandRotation, t);
        //}

        //if (!grabingLedge) {
        //    if (Input.GetKey(KeyCode.Mouse1)) {
        //        if (LookForGrip())
        //            GrabLedge();
        //    }
        //}
    }

    //private Vector3 WallClimbUpdate() {

    //    Vector3 position = tIkTarget.position;
    //    RaycastHit wallHit = character.locomotion.wallrunController.wallHit;

    //    // Raycast down from foot
    //    RaycastHit hit;
    //    if (Physics.Raycast(character.body.postAnimationState.GetBoneState(Body.BoneEnums.rHandR).position + wallHit.normal, -wallHit.normal, out hit, 4, LayerMasks.i.environment)) {
    //        position = hit.point;
    //        Debug.Log("HIT");
    //    }
    //    Debug.Log("MISS");

    //    return position;

    //    //tPhysicalTarget.rotation = tHand.rotation;

    //    //// Looking left or right
    //    //if (character.locomotion.wallrunController.wallCameraAngle < -10 && side == Enums.Side.right || character.locomotion.wallrunController.wallCameraAngle > 10 && side == Enums.Side.left) {

    //    //    Vector3 sideLookHandPosition = character.head.tCamera.position + character.head.tCamera.TransformVector(handCameraPosition);
    //    //    Quaternion sideLookHandRotation = character.head.tCamera.rotation * Quaternion.Euler(handCameraRotation);

    //    //    float t = wallAngleToHandLookCurve.Evaluate(Mathf.Abs(character.locomotion.wallrunController.wallCameraAngle));
    //    //    transform.position = Vector3.Lerp(transform.position, sideLookHandPosition, t);
    //    //    transform.rotation = Quaternion.Slerp(transform.rotation, sideLookHandRotation, t);
    //    //}

    //    //if (!grabingLedge) {
    //    //    if (Input.GetKey(KeyCode.Mouse1)) {
    //    //        if (LookForGrip())
    //    //            GrabLedge();
    //    //    }
    //    //}
    //}

    private void PhysicalHandUpdate() {
        kmTarget.DoUpdate();

        velocity = rb.velocity;

        targetDeltaPos = VectorUtils.FromToVector(transform.position, kmTarget.transform.position);

        //targetDeltaPos = VectorUtils.FromToVector(character.body.tHandR.position, kmTarget.transform.position);
        //targetDeltaPos = VectorUtils.FromToVector(tWeaponTarget.position, kmTarget.transform.position);

        targetVelocity = kmTarget.velocity + targetDeltaPos * errorAdjustmentCoef;
        velocity = Vector3.Lerp(velocity, targetVelocity, Time.fixedDeltaTime * velocityChangeCoef);
        rb.velocity = velocity;

        //if (grabingLedge)
        //    targetDeltaPos = VectorUtils.FromToVector(transform.position, character.transform.position);
    }

    private void WeaponController_reloadStartedEvent(float reloadTime) {
        reloadCorutine = character.StartCoroutine(ReloadCorutine(reloadTime));
    }

    private IEnumerator ReloadCorutine(float reloadTime) {
        float t = 0;

        while (t < 1) {
            t += Time.deltaTime / reloadTime;
            reloadSpinPitch = 1440 * t;
            yield return new WaitForEndOfFrame();
        }

        reloadSpinPitch = 0;
    }

    private bool LookForGrip() {
        Vector3 direction = -character.locomotion.wallrunController.wallHit.normal;
        Vector3 position = new Vector3(character.transform.position.x, tPhysicalTarget.position.y, character.transform.position.z);

        RaycastHit hit1;
        if (Physics.Raycast(tPhysicalTarget.position - Vector3.up * 0.025f - direction * 0.5f, direction, out hit1, 1f, LayerMasks.i.environment)) {
            RaycastHit hit2;
            if (Physics.Raycast(hit1.point + direction * 0.05f + Vector3.up * 0.1f, Vector3.down, out hit2, 0.2f, LayerMasks.i.environment)) {
                if (Vector3.Angle(hit2.normal, Vector3.up) < 15f && Vector3.Angle(hit2.normal, direction) > 75f) {
                    Debug.Log("GRIP FOUND!");
                    if (!grabingLedge)
                        character.locomotion.wallrunController.t = 0.0f;
                    grabingLedge = true;
                    grabPoint = hit2.point + Vector3.up * 0.005f;
                    grabRotation = Quaternion.LookRotation(-Vector3.Cross(hit1.normal, hit2.normal), -hit1.normal);

                    //grabRotation = tPhysicalTarget.rotation;
                    //tPhysicalTarget.position = hit2.point;
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
    //    if (Physics.Raycast(tPhysicalTarget.position - Vector3.up * 0.025f - direction * 0.1f, direction, out hit1, 0.3f, LayerMasks.i.environment)) {
    //        RaycastHit hit2;
    //        if (Physics.Raycast(hit1.point + direction * 0.05f + Vector3.up * 0.1f, Vector3.down, out hit2, 0.2f, LayerMasks.i.environment)) {
    //            if (Vector3.Angle(hit2.normal, Vector3.up) < 15f && Vector3.Angle(hit2.normal, direction) > 75f) {
    //                Debug.Log("GRIP FOUND!");
    //                tPhysicalTarget.position = hit2.point;
    //                return true;
    //            }
    //        }

    //        return false;
    //    }
    //    else
    //        return false;
    //}

    private void GrabLedge() {
        grabingLedge = true;
        Debug.Log("Gripped");
        //edgeGrabPoint = transform.position;
        //goGripIndicator.SetActive(true);

        ////SoftJointLimit limit = hangJoint.linearLimit;
        ////limit.limit = 0.65f;
        ////hangJoint.linearLimit = limit;

        //ledgeGrabedEvent?.Invoke();
    }

}
