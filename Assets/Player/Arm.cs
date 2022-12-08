using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Arm {
    [SerializeField] private Transform tBase;
    [SerializeField] private Transform tHandTarget;

    private Player player;

    public Vector3 handRotationOffset;

    public void Initialize(Player player) {
        this.player = player;

        player.fixedUpdateEvent += Player_fixedUpdateEvent;
    }

    private void Player_fixedUpdateEvent() {
        tBase.position = player.fpCamera.tHead.position;
        tBase.rotation = Quaternion.Lerp(tBase.rotation, player.fpCamera.tHead.rotation, Time.fixedDeltaTime * 20);

        handRotationOffset = new Vector3(handRotationOffset.x, handRotationOffset.y, Mathf.Lerp(handRotationOffset.z, -player.playerController.inputDir.x * 25 + player.rb.angularVelocity.y * 200, Time.deltaTime * 8));

        tHandTarget.rotation = player.fpCamera.tHead.rotation;
        tHandTarget.Rotate(handRotationOffset);
    }
}
