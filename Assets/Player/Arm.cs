using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Arm {
    [SerializeField] private Transform tBase;
    [SerializeField] private Transform tHandTarget;

    public Vector3 hipHandPosition;
    public Vector3 adsHandPosition;

    private Character character;

    public Vector3 handRotationOffset;

    private TWrapper hipAdsInterpolator = new TWrapper(0, 1, 0);
    private Coroutine interpolationCorutine;

    public void Init(Character character) {
        this.character = character;

        hipHandPosition = tHandTarget.localPosition;

        character.fixedUpdateEvent += Player_fixedUpdateEvent;

        character.weaponController.adsEnteredEvent += WeaponController_adsEnteredEvent;
        character.weaponController.adsExitedEvent += WeaponController_adsExitedEvent;
    }

    private void Player_fixedUpdateEvent() {
        tBase.position = character.fpCamera.tHead.position;
        tBase.rotation = Quaternion.Lerp(tBase.rotation, character.fpCamera.tHead.rotation, Time.fixedDeltaTime * 20);

        handRotationOffset = new Vector3(handRotationOffset.x, handRotationOffset.y, Mathf.Lerp(handRotationOffset.z, -character.characterInput.moveInput.x * 25 + character.rb.angularVelocity.y * 200, Time.deltaTime * 8));

        tHandTarget.localPosition = Vector3.Lerp(hipHandPosition, adsHandPosition, hipAdsInterpolator.t);
        tHandTarget.rotation = character.fpCamera.tHead.rotation;
        tHandTarget.Rotate(handRotationOffset);
    }

    private void WeaponController_adsEnteredEvent() {
        if (interpolationCorutine != null)
            character.StopCoroutine(interpolationCorutine);

        interpolationCorutine = character.StartCoroutine(InterpolationUtils.i.SmoothStep(hipAdsInterpolator.t, 1, 4, hipAdsInterpolator));
    }

    private void WeaponController_adsExitedEvent() {
        if (interpolationCorutine != null)
            character.StopCoroutine(interpolationCorutine);

        interpolationCorutine = character.StartCoroutine(InterpolationUtils.i.SmoothStep(hipAdsInterpolator.t, 0, 4, hipAdsInterpolator));
    }

}
