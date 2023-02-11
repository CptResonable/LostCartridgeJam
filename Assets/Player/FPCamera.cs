using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FPCamera {
    public Transform tCameraTarget;
    public Transform tCamera;

    public float yaw, pitch, roll;

    // Recoil
    [SerializeField] private AnimationCurve recoilApplicationCurve;

    // Wall running
    [SerializeField] private AnimationCurve wallRunVerticalTiltBackCurve;
    [SerializeField] private AnimationCurve wallAngleToRollCurve;

    private Camera camera;
    private Character character;

    public void Initialize(Character character) {
        this.character = character;
        tCamera.TryGetComponent<Camera>(out camera);

        character.updateEvent += character_updateEvent;
        character.fixedUpdateEvent += character_fixedUpdateEvent;

        if (character.weaponController.rifle != null)
            character.weaponController.rifle.gunFiredEvent += EquipedGun_gunFiredEvent;

        if (character.weaponController.pistol != null)
            character.weaponController.pistol.gunFiredEvent += EquipedGun_gunFiredEvent;

        character.locomotion.wallrunController.verticalRunStarted += WallrunController_verticalRunStarted;
    }

    private void character_fixedUpdateEvent() {
    }

    public void character_updateEvent() {
        pitch -= character.characterInput.mouseMovement.yDelta * Settings.MOUSE_SENSITIVITY;
        pitch = Mathf.Clamp(pitch, -89, 89);
        yaw += character.characterInput.mouseMovement.xDelta * Settings.MOUSE_SENSITIVITY;

        float targetRoll = 0;
        if (Input.GetKey(KeyCode.Q))
            targetRoll += 30;
        else if (Input.GetKey(KeyCode.E))
            targetRoll -= 30;

        if (character.locomotion.wallrunController.isWallRunning) {

            // Y angle between look direction and -wall normal
            targetRoll += Mathf.Sign(character.locomotion.wallrunController.wallCameraAngle) * wallAngleToRollCurve.Evaluate(Mathf.Abs(character.locomotion.wallrunController.wallCameraAngle)) * 40;
        }

        roll = Mathf.Lerp(roll, targetRoll, Time.deltaTime * 12);

        if (camera != null) {
            camera.fieldOfView = Mathf.Lerp(Settings.FOV_HIP, Settings.FOV_ADS, character.arms.hipAdsInterpolator.t);
        }

        //Vector3 lastForwardVector = Vector3.ProjectOnPlane(tCamera.transform.forward, Vector3.up);
        Vector3 lastForwardVector = new Vector3(tCamera.transform.forward.x, 0, tCamera.transform.forward.z);
        tCamera.rotation = Quaternion.identity;
        tCamera.Rotate(lastForwardVector.normalized, roll * lastForwardVector.magnitude, Space.World);
        tCamera.Rotate(Vector3.up, yaw, Space.Self);
        tCamera.Rotate(Vector3.right, pitch, Space.Self);
        //tCamera.rotation = Quaternion.Euler(pitch, yaw, roll);

    }

    private void EquipedGun_gunFiredEvent(Vector3 rotationalRecoil, Vector3 translationalRecoil) {
        character.StartCoroutine(ApplyRotationOverTime(-rotationalRecoil.x, rotationalRecoil.y, 0.12f, recoilApplicationCurve));
    }

    public void SetRotation() {
        tCameraTarget.parent.rotation = Quaternion.Euler(pitch, yaw, 0);
    }


    private void WallrunController_verticalRunStarted() {
        character.StartCoroutine(ApplyRotationOverTime(-25, 0, 0.35f, wallRunVerticalTiltBackCurve));
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
