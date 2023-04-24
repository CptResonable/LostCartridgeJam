using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Head {
    public Transform tCameraTarget_hip;
    public Transform tCameraTarget_ads;
    public Transform tCameraBase;

    public float yaw, pitch, roll;

    private Character character;

    public void Initialize(Character character) {
        this.character = character;

        character.updateEvent += character_updateEvent;
        character.fixedUpdateEvent += character_fixedUpdateEvent;

        character.equipmentManager.itemEquipedEvent += EquipmentManager_itemEquipedEvent;
        character.equipmentManager.itemUnequipedEvent += EquipmentManager_itemUnequipedEvent;

        character.locomotion.wallrunController.verticalRunStarted += WallrunController_verticalRunStarted;
        character.locomotion.wallrunController.verticalRunStopped += WallrunController_verticalRunStopped;
        character.locomotion.wallrunController.horizontalRunStarted += WallrunController_horizontalRunStarted;
    }


    private void character_fixedUpdateEvent() {
        character.head.tCameraBase.position = Vector3.Lerp(character.head.tCameraTarget_hip.position, character.head.tCameraTarget_ads.position, character.stanceController.hipAdsInterpolator.t); // Set camera position, needs to be done here aswell for camera and hands to move smoothly
    }

    float tiltAmount;
    public void character_updateEvent() {

        float lastYaw = yaw;
        pitch -= character.characterInput.mouseMovement.yDelta * Settings.MOUSE_SENSITIVITY;
        pitch = Mathf.Clamp(pitch, -89, 89);

        float yawChange = character.characterInput.mouseMovement.xDelta * Settings.MOUSE_SENSITIVITY;
        yaw += yawChange;

        if (yaw > 360)
            yaw -= 360;
        if (yaw <= 0)
            yaw = 360 + yaw;

        if (character.locomotion.activeStateEnum == Locomotion.LocomotionState.StateIDEnum.WallRunning) {
            float angle = Vector3.SignedAngle(Vector3.forward, character.locomotion.wallrunController.wallForwardVector, Vector3.up);
            angle += 360;
            float deltaAngle = Mathf.DeltaAngle(angle, yaw);

            if (deltaAngle > 88)
                yaw = angle + 88;
            if (deltaAngle < -88)
                yaw = angle - 88;

            if (character.locomotion.wallrunController.wallRunSide == Enums.Side.left)
                tiltAmount = -160;
            else
                tiltAmount = 160;
        }
        else if (character.locomotion.activeStateEnum == Locomotion.LocomotionState.StateIDEnum.WallClimbing) {
            float angle = Vector3.SignedAngle(Vector3.forward, character.locomotion.wallrunController.wallHit.normal, Vector3.up) + 180;

            float dl = Mathf.DeltaAngle(lastYaw, angle);
            float dn = Mathf.DeltaAngle(yaw, angle);
            if (Mathf.Sign(dl) != Mathf.Sign(dn) && Mathf.Abs(dl) > 90)
                yaw = lastYaw;

            tiltAmount = -Mathf.DeltaAngle(yaw, angle - 180);
        }

        float targetRoll = 0;
        if (character.characterInput.action_leanLeft.isDown)
            targetRoll += 30;
        else if (character.characterInput.action_leanRight.isDown)
            targetRoll -= 30;

        if (character.locomotion.wallrunController.isWallRunning) {

            // Y angle between look direction and -wall normal
            targetRoll += Mathf.Sign(tiltAmount) * MiscAnimationCurves.i.wallAngleToRollCurve.Evaluate(Mathf.Abs(character.locomotion.wallrunController.wallCameraAngle)) * 30;
        }

        roll = Mathf.Lerp(roll, targetRoll, Time.deltaTime * 12);

        //Vector3 lastForwardVector = Vector3.ProjectOnPlane(tCamera.transform.forward, Vector3.up);
        Vector3 lastForwardVector = new Vector3(tCameraBase.transform.forward.x, 0, tCameraBase.transform.forward.z);
        tCameraBase.rotation = Quaternion.identity;

        tCameraBase.Rotate(lastForwardVector.normalized, roll * lastForwardVector.magnitude, Space.Self);
        tCameraBase.Rotate(Vector3.up, yaw, Space.Self);
        tCameraBase.Rotate(Vector3.right, pitch, Space.Self);
        //tCamera.rotation = Quaternion.Euler(pitch, yaw, roll);
    }

    public void PostDamageReactionUpdate() {
        character.body.tHead.rotation = character.head.tCameraBase.rotation; // Set head rotation to camera rotation
        character.body.tHead.Rotate(character.body.tHead.forward, Mathf.Lerp(0, -30, character.stanceController.hipAdsInterpolator.t), Space.World);
        character.head.tCameraBase.position = Vector3.Lerp(character.head.tCameraTarget_hip.position, character.head.tCameraTarget_ads.position, character.stanceController.hipAdsInterpolator.t); // Set camera position
    }

    private void EquipmentManager_itemEquipedEvent(Equipment item) {
        if (item.GetType() == typeof(Gun)) {
            Gun gun = (Gun)item;
            gun.gunFiredEvent += Gun_gunFiredEvent;
        }
    }

    private void EquipmentManager_itemUnequipedEvent(Equipment item) {
        if (item.GetType() == typeof(Gun)) {
            Gun gun = (Gun)item;
            gun.gunFiredEvent -= Gun_gunFiredEvent;
        }
    }

    private void Gun_gunFiredEvent(Vector3 rotationalRecoil, Vector3 translationalRecoil) {
        character.StartCoroutine(ApplyRotationOverTime(-rotationalRecoil.x, rotationalRecoil.y, 0.12f, MiscAnimationCurves.i.recoilHeadApplicationCurve));
        //shaker.Shake(2, 0.12f);
    }

    //public void SetRotation() {
    //    tCameraTarget.parent.rotation = Quaternion.Euler(pitch, yaw, 0);
    //}

    private void WallrunController_verticalRunStarted() {
        character.StartCoroutine(ApplyRotationOverTime(-25, 0, 0.35f, MiscAnimationCurves.i.wallRunVerticalTiltBackCurve));
    }

    private void WallrunController_verticalRunStopped() {
    }

    private void WallrunController_horizontalRunStarted() {
        Vector3 camForwardProj = Vector3.ProjectOnPlane(tCameraBase.forward, character.locomotion.wallrunController.wallUpVector).normalized;
        float angle = Vector3.SignedAngle(character.locomotion.wallrunController.wallForwardVector, camForwardProj, character.locomotion.wallrunController.wallUpVector);
        if (Vector3.Dot(camForwardProj, character.locomotion.wallrunController.wallHit.normal) < 0)
            character.StartCoroutine(ApplyRotationOverTime(0, -angle, 0.35f, MiscAnimationCurves.i.wallRunVerticalTiltBackCurve));
    }

    public IEnumerator ApplyRotationOverTime(float totalPitch, float totalYaw, float time, AnimationCurve applicationCurve) {
        float t = 0;
        float lastValue = 0;

        while (t < 1) {
            t += Time.fixedDeltaTime / time;

            float value = applicationCurve.Evaluate(t);
            float dValue = value - lastValue;
            lastValue = value;

            pitch += dValue * totalPitch;
            yaw += dValue * totalYaw;

            yield return new WaitForFixedUpdate();
        }
    }
}
