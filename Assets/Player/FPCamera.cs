using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FPCamera {
    public Transform tCameraTarget;
    public Transform tCamera;
    public Camera camera;
    public Animator animator;

    public float yaw, pitch, roll;

    // Recoil
    [SerializeField] private AnimationCurve recoilApplicationCurve;

    // Wall running
    [SerializeField] private AnimationCurve wallRunVerticalTiltBackCurve;
    [SerializeField] private AnimationCurve wallAngleToRollCurve;

    private Shaker shaker;

    private float headbobAmount = 0;

    private Character character;

    public void Initialize(Character character) {
        this.character = character;

        shaker = camera.GetComponent<Shaker>();

        character.updateEvent += character_updateEvent;
        character.lateUpdateEvent += Character_lateUpdateEvent;
        character.fixedUpdateEvent += character_fixedUpdateEvent;

        if (character.weaponController.rifle != null)
            character.weaponController.rifle.gunFiredEvent += EquipedGun_gunFiredEvent;

        if (character.weaponController.pistol != null)
            character.weaponController.pistol.gunFiredEvent += EquipedGun_gunFiredEvent;

        character.locomotion.wallrunController.verticalRunStarted += WallrunController_verticalRunStarted;
        character.locomotion.wallrunController.verticalRunStopped += WallrunController_verticalRunStopped;
        character.locomotion.wallrunController.horizontalRunStarted += WallrunController_horizontalRunStarted;
        character.locomotion.wallrunController.horizontalRunStopped += WallrunController_horizontalRunStopped;
        character.locomotion.slideStartedEvent += Locomotion_slideStartedEvent;
        character.locomotion.slideEndedEvent += Locomotion_slideEndedEvent;
    }

    private void Character_lateUpdateEvent() {
        //camera.transform.localPosition -= Vector3.up * 0.5f;
    }

    private void character_fixedUpdateEvent() {
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

            //if (deltaAngle > 89)
            //    yaw = angle + 89;
            //if (deltaAngle < -89)
            //    yaw = angle - 89;
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
            targetRoll += Mathf.Sign(tiltAmount) * wallAngleToRollCurve.Evaluate(Mathf.Abs(character.locomotion.wallrunController.wallCameraAngle)) * 30;
        }

        roll = Mathf.Lerp(roll, targetRoll, Time.deltaTime * 12);

        if (camera != null) {
            camera.fieldOfView = Mathf.Lerp(Settings.FOV_HIP, Settings.FOV_ADS, character.arms.hipAdsInterpolator.t);
        }

        //Vector3 lastForwardVector = Vector3.ProjectOnPlane(tCamera.transform.forward, Vector3.up);
        Vector3 lastForwardVector = new Vector3(tCamera.transform.forward.x, 0, tCamera.transform.forward.z);
        tCamera.rotation = Quaternion.identity;

        tCamera.Rotate(lastForwardVector.normalized, roll * lastForwardVector.magnitude, Space.Self);
        tCamera.Rotate(Vector3.up, yaw, Space.Self);
        tCamera.Rotate(Vector3.right, pitch, Space.Self);
        //tCamera.rotation = Quaternion.Euler(pitch, yaw, roll);

        headbobAmount = Mathf.Lerp(headbobAmount, character.rb.velocity.magnitude / 4, Time.deltaTime * 4);
        animator.SetFloat("Velocity", headbobAmount);
    }

    //public void character_updateEvent() {
    //    pitch -= character.characterInput.mouseMovement.yDelta * Settings.MOUSE_SENSITIVITY;
    //    pitch = Mathf.Clamp(pitch, -89, 89);
    //    yaw += character.characterInput.mouseMovement.xDelta * Settings.MOUSE_SENSITIVITY;


    //    if (character.locomotion.activeStateEnum == Locomotion.LocomotionState.StateIDEnum.WallRunning) {
    //        float angle = Vector3.SignedAngle(Vector3.forward, character.locomotion.wallrunController.wallForwardVector, Vector3.up);
    //        angle += 360;
    //        float deltaAngle = Mathf.DeltaAngle(angle, yaw);

    //        //if (deltaAngle > 89)
    //        //    yaw = angle + 89;
    //        //if (deltaAngle < -89)
    //        //    yaw = angle - 89;
    //        if (deltaAngle > 88)
    //            yaw = angle + 88;
    //        if (deltaAngle < -88)
    //            yaw = angle - 88;
    //    }
    //    //else if (character.locomotion.activeStateEnum == Locomotion.LocomotionState.StateIDEnum.WallClimbing) {
    //    //    float angle = Vector3.SignedAngle(Vector3.forward, character.locomotion.wallrunController.wallHit.normal, Vector3.up) + 90;
    //    //    angle += 360;
    //    //    float deltaAngle = Mathf.DeltaAngle(angle, yaw);

    //    //    //if (deltaAngle > 89)
    //    //    //    yaw = angle + 89;
    //    //    //if (deltaAngle < -89)
    //    //    //    yaw = angle - 89;
    //    //    if (deltaAngle > 88)
    //    //        yaw = angle + 88;
    //    //    if (deltaAngle < -88)
    //    //        yaw = angle - 88;
    //    //}

    //    //if (character.locomotion.activeStateEnum == Locomotion.LocomotionState.StateIDEnum.WallRunning) {
    //    //    float angle = Vector3.SignedAngle(Vector3.forward, character.locomotion.wallrunController.wallForwardVector, Vector3.up);
    //    //    if (angle < 0)
    //    //        angle = 360 + angle;

    //    //    Debug.Log("angel: " + angle);

    //    //    yaw += 360;
    //    //    angle += 360;

    //    //    //yaw = angle;

    //    //    yaw = Mathf.Clamp(yaw, angle - 90, angle + 90);
    //    //    yaw -= 360;
    //    //}


    //    if (yaw > 360)
    //        yaw -= 360;
    //    if (yaw < 0)
    //        yaw = 360 + yaw;

    //    Debug.Log("yaw: " + yaw);


    //    float targetRoll = 0;
    //    if (Input.GetKey(KeyCode.Q))
    //        targetRoll += 30;
    //    else if (Input.GetKey(KeyCode.E))
    //        targetRoll -= 30;

    //    if (character.locomotion.wallrunController.isWallRunning) {

    //        // Y angle between look direction and -wall normal
    //        targetRoll += Mathf.Sign(character.locomotion.wallrunController.wallCameraAngle) * wallAngleToRollCurve.Evaluate(Mathf.Abs(character.locomotion.wallrunController.wallCameraAngle)) * 40;
    //    }

    //    roll = Mathf.Lerp(roll, targetRoll, Time.deltaTime * 12);

    //    if (camera != null) {
    //        camera.fieldOfView = Mathf.Lerp(Settings.FOV_HIP, Settings.FOV_ADS, character.arms.hipAdsInterpolator.t);
    //    }

    //    //Vector3 lastForwardVector = Vector3.ProjectOnPlane(tCamera.transform.forward, Vector3.up);
    //    Vector3 lastForwardVector = new Vector3(tCamera.transform.forward.x, 0, tCamera.transform.forward.z);
    //    tCamera.rotation = Quaternion.identity;

    //    tCamera.Rotate(lastForwardVector.normalized, roll * lastForwardVector.magnitude, Space.Self);
    //    tCamera.Rotate(Vector3.up, yaw, Space.Self);
    //    tCamera.Rotate(Vector3.right, pitch, Space.Self);
    //    //tCamera.rotation = Quaternion.Euler(pitch, yaw, roll);

    //    headbobAmount = Mathf.Lerp(headbobAmount, character.rb.velocity.magnitude / 4, Time.deltaTime * 4);
    //    animator.SetFloat("Velocity", headbobAmount);
    //}

    private void EquipedGun_gunFiredEvent(Vector3 rotationalRecoil, Vector3 translationalRecoil) {
        character.StartCoroutine(ApplyRotationOverTime(-rotationalRecoil.x, rotationalRecoil.y, 0.12f, recoilApplicationCurve));
        //shaker.Shake(2, 0.12f);
    }

    //public void SetRotation() {
    //    tCameraTarget.parent.rotation = Quaternion.Euler(pitch, yaw, 0);
    //}

    private void WallrunController_verticalRunStarted() {
        character.StartCoroutine(ApplyRotationOverTime(-25, 0, 0.35f, wallRunVerticalTiltBackCurve));
        animator.SetBool("IsWallClimbing", true);
    }

    private void WallrunController_verticalRunStopped() {
        animator.SetBool("IsWallClimbing", false);
    }


    private void WallrunController_horizontalRunStarted() {
        Vector3 camForwardProj = Vector3.ProjectOnPlane(tCamera.forward, character.locomotion.wallrunController.wallUpVector).normalized;
        float angle = Vector3.SignedAngle(character.locomotion.wallrunController.wallForwardVector, camForwardProj, character.locomotion.wallrunController.wallUpVector);
        if (Vector3.Dot(camForwardProj, character.locomotion.wallrunController.wallHit.normal) < 0)
            character.StartCoroutine(ApplyRotationOverTime(0, -angle, 0.35f, wallRunVerticalTiltBackCurve));
    }

    private void WallrunController_horizontalRunStopped() {
    }

    private void Locomotion_slideStartedEvent() {
        animator.SetBool("IsSliding", true);
    }

    private void Locomotion_slideEndedEvent() {
        animator.SetBool("IsSliding", false);
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

    public void Recoil(float pitch, float yaw) {
        this.pitch += pitch;
        this.yaw += yaw;
        this.pitch = Mathf.Clamp(pitch, -89, 89);
    }
}
