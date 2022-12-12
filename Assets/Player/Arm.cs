using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Arm {
    [SerializeField] private Transform tBase;
    [SerializeField] private Transform tHandTarget;

    private Character character;

    public Vector3 handRotationOffset;

    public void Initialize(Character character) {
        this.character = character;

        character.fixedUpdateEvent += Player_fixedUpdateEvent;
    }

    private void Player_fixedUpdateEvent() {
        tBase.position = character.fpCamera.tHead.position;
        tBase.rotation = Quaternion.Lerp(tBase.rotation, character.fpCamera.tHead.rotation, Time.fixedDeltaTime * 20);

        handRotationOffset = new Vector3(handRotationOffset.x, handRotationOffset.y, Mathf.Lerp(handRotationOffset.z, -character.characterInput.moveInput.x * 25 + character.rb.angularVelocity.y * 200, Time.deltaTime * 8));

        tHandTarget.rotation = character.fpCamera.tHead.rotation;
        tHandTarget.Rotate(handRotationOffset);
    }
}
