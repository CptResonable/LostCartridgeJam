using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FPCamera {
    public Transform tCameraTarget;
    public Transform tCamera;

    public float yaw, pitch, roll;

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
        character.StartCoroutine(ApplyRecoilCorutine(-rotationalRecoil.x, rotationalRecoil.y, 0.12f));
    }

    public void SetRotation() {
        tCameraTarget.parent.rotation = Quaternion.Euler(pitch, yaw, 0);
    }

    //public IEnumerator ApplyRecoilCorutine(float pitch, float yaw, float time) {
    //    float t = 0;

    //    while (t < 1) {
    //        float dTime =
    //        t += Time.fixedDeltaTime / time;

    //        yield return new WaitForFixedUpdate();
    //    }
    //}

    [SerializeField] private AnimationCurve recoilApplicationCurve;

    public IEnumerator ApplyRecoilCorutine(float pitchRecoil, float yawRecoil, float time) {
        float t = 0;
        float lastValue = 0;

        while (t < 1) {
            t += Time.fixedDeltaTime / time;

            float value = recoilApplicationCurve.Evaluate(t);
            float dValue = value - lastValue;
            lastValue = value;

            pitch += dValue * pitchRecoil;
            yaw += dValue * yawRecoil;

            yield return new WaitForFixedUpdate();
        }
    }

    public void Recoil(float pitch, float yaw) {
        this.pitch += pitch;
        this.yaw += yaw;
        this.pitch = Mathf.Clamp(pitch, -89, 89);
    }
}
